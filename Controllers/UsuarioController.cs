using ConsultorioMedicoVerde.Models;
using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ConsultorioMedicoVerde.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuarioController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        public UsuarioController(ApiServiceProxy apiProxy)
        {
            _apiProxy = apiProxy;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string buscar, string filtro, bool mostrarActivos = true)
        {
            // Enviamos el estado del switch a la vista para que el checkbox sepa cómo marcarse
            ViewBag.MostrarActivos = mostrarActivos;
            ViewBag.Buscar = buscar;
            ViewBag.Filtro = filtro;

            // 1. Si 'buscar' es estrictamente NULL, es la primera entrada. 
            //if (filtro == null)
            //{
            //    return View(new List<UsuarioViewModel>());
            //}

            // 2. Si llegamos aquí, es porque el usuario dio clic en "Filtrar" o "Refrescar".
            var busqueda = new UsuarioViewModel { IdUsuario = 0 };

            if (!string.IsNullOrEmpty(buscar))
            {
                if (filtro == "Nombre")
                    busqueda.NombreUsuario = buscar;
                else if (filtro == "Rol")
                    busqueda.Rol = buscar;
            }
            // Llamada al API (si buscar es "", mandamos el objeto limpio y trae todos)
            var lista = await _apiProxy.SendRequestAsync<List<UsuarioViewModel>>("Usuario", "ListarUsuario", HttpMethod.Post, busqueda);
           
            return View(lista ?? new List<UsuarioViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarMedicos();
            var nuevoUsuario = new UsuarioViewModel
            {
                Activo = true,
                IdUsuario = 0,
                Rol = ""
            };
            return View("Formulario", nuevoUsuario);
        }


        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            // 1. Creamos el objeto de búsqueda con el ID que viene de la URL
            var busqueda = new UsuarioViewModel { IdUsuario = id };

            // 2. Enviamos el objeto al API. 
            // Es vital que el API reciba este ID para que filtre en la base de datos.
            var lista = await _apiProxy.SendRequestAsync<List<UsuarioViewModel>>(
                "Usuario",
                "ListarUsuario",
                HttpMethod.Post,
                busqueda
            );

            // 3. Tomamos el usuario resultante
            var usuario = lista.Where((e)=> e.IdUsuario == id);

            if (!usuario.Any())
            {
                TempData["Error"] = "No se encontró el usuario solicitado.";
                return RedirectToAction("Index");
            }

            // 4. Cargamos la lista de médicos para el dropdown
            await CargarMedicos();

            var userEdit = usuario.First();
            userEdit.Usuario = userEdit.NombreUsuario;
            userEdit.Contrasena = userEdit.Password;
            userEdit.UsuarioModificacion = User.Identity?.Name ?? "Sistema";
            userEdit.FechaModificacion = DateTime.Now;


            // 5. Devolvemos la vista Formulario con los datos del usuario correcto
            return View("Formulario", userEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarEdicion(UsuarioViewModel model)
        {
            // Capturamos el ID del usuario logueado (desde Claims o Identity)
            var idUsuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

            // Asignación de Auditoría
            model.UsuarioModificacion = idUsuarioLogueado;
            model.FechaModificacion = DateTime.Now;
            model.Usuario = model.NombreUsuario;
            model.Contrasena = model.Password;

            // 3. Quitar el error de validación de UsuarioModificacion porque ya lo llenamos
            ModelState.Remove("UsuarioModificacion");
            ModelState.Remove("UsuarioCreacion");
            ModelState.Remove("Contrasena");
            ModelState.Remove("Usuario");

            if (model.IdUsuario == 0 && string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("password", "La contraseña es obligatoria");
            }

            // Preparar objeto para el API (basado en el curl que pasaste)
            var busqueda = new UsuarioViewModel
            {
                IdUsuario = model.IdUsuario, 
                NombreUsuario = model.NombreUsuario,
                Password = model.Password,
                Rol = model.Rol,
                IdMedico = model.IdMedico,
                FechaModificacion = model.FechaModificacion,
                Usuario = model.Usuario,
                Activo = model.Activo,
                Contrasena = model.Contrasena,
                UsuarioModificacion = model.UsuarioModificacion

            };

            var respuesta = await _apiProxy.SendRequestAsync<object>("Usuario", "ActualizarUsuario", HttpMethod.Put, busqueda);

            if (respuesta != null)
            {
                TempData["Mensaje"] = "Operación realizada con éxito.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = "No se completo Actualización del usuario";

            ModelState.AddModelError("", "Error en el API al procesar el usuario.");
            await CargarMedicos();
            return View("Formulario", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(UsuarioViewModel model)
        {
            // Capturamos el ID del usuario logueado (desde Claims o Identity)
            var idUsuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

            // Asignación de Auditoría
            model.UsuarioCreacion = User.Identity?.Name ?? "Sistema";
            model.UsuarioModificacion = idUsuarioLogueado;
            model.FechaModificacion = DateTime.Now;
            model.FechaCreacion = DateTime.Now;
            model.Usuario = model.NombreUsuario;
            model.Contrasena = model.Password;

            // 3. Quitar el error de validación de UsuarioModificacion porque ya lo llenamos
            ModelState.Remove("UsuarioModificacion");
            ModelState.Remove("UsuarioCreacion");
            ModelState.Remove("Contrasena");
            ModelState.Remove("Usuario");

            if (model.IdUsuario == 0 && string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("password", "La contraseña es obligatoria");
            }

            if (!ModelState.IsValid)
            {
                await CargarMedicos();
                return View("Formulario", model);
            }

            // Preparar objeto para el API (basado en el curl que pasaste)
            var busqueda = new UsuarioViewModel
            {
             // IdUsuario = model.IdUsuario, 
              NombreUsuario  = model.NombreUsuario,
              Password= model.Password,
              Rol= model.Rol,
              IdMedico = model.IdMedico,
              FechaCreacion = model.FechaCreacion,
              UsuarioCreacion= model.UsuarioCreacion,
              FechaModificacion= model.FechaModificacion,
              Usuario= model.Usuario,
              Activo= model.Activo,
              Contrasena= model.Contrasena,
              UsuarioModificacion = model.UsuarioModificacion

            };

            var respuesta = await _apiProxy.SendRequestAsync<object>("Usuario", "InsertarUsuario", HttpMethod.Post, busqueda);

            if (respuesta != null)
            {
                TempData["Mensaje"] = "Operación realizada con éxito.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = "No se completo creacion del usuario";

            ModelState.AddModelError("", "Error en el API al procesar el usuario.");
            await CargarMedicos();
            return View("Formulario", model);
        }

        private async Task CargarMedicos()
        {
            try
            {
                var busquedaMedico = new { idMedico = 0 };
                // Usamos dynamic o una clase MedicoViewModel para mapear el endpoint de médicos
              //  var medicos = await _apiProxy.SendRequestAsync<List<dynamic>>("Medico", "ListarMedico", HttpMethod.Post, busquedaMedico);
                var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post);

                if (medicos != null)
                {
                    // IMPORTANTE: Ajusta "Nombre" y "Apellido" según devuelva tu API de médicos
                    ViewBag.Medicos = medicos.Select(m => new SelectListItem
                    {
                        Value = m.IdMedico.ToString(),
                        Text = $"{m.Nombre} {m.Apellido}"
                    }).ToList();
                }
                else
                {
                    ViewBag.Medicos = new List<SelectListItem>();
                }
            }
            catch
            {
                ViewBag.Medicos = new List<SelectListItem>();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, bool activo, string buscar, string filtro, bool mostrarActivos)
        {
            try
            {

                var usuarios = await _apiProxy.SendRequestAsync<List<UsuarioViewModel>>("Usuario", "ListarUsuario", HttpMethod.Post);
                var usuario = usuarios?.FirstOrDefault(p => p.IdUsuario == id);

                if (usuario == null)
                {
                    TempData["Error"] = "No se encontró el usuario.";
                    return RedirectToAction("Index");
                }

                // 2. Actualizamos solo el estado y los campos de auditoría
                usuario.Activo = activo;
                usuario.UsuarioModificacion = User.Identity?.Name ?? "Sistema";
                usuario.FechaModificacion = DateTime.Now;

                // 3. Enviamos el objeto al endpoint PUT que indicaste
                //var resultado = await _apiProxy.SendRequestAsync<dynamic>("Usuario", "ActualizarEstadoUsuario", HttpMethod.Put, usuario  );
                var resultado = await _apiProxy.SendRequestAsync<object>("Usuario", "ActualizarEstadoUsuario", HttpMethod.Put, usuario);

                TempData["Mensaje"] = activo
                    ? $"El usuario {usuario.NombreUsuario} ha sido activado."
                    : $"El usuario {usuario.NombreUsuario} ha sido desactivado.";

                // Redirigimos pasando los mismos filtros para que el Index los procese
                return RedirectToAction("Index", new { buscar = buscar, filtro = filtro, mostrarActivos = mostrarActivos });

            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el estado: " + ex.Message;
            }

            // REGRESA AUTOMÁTICAMENTE A LA LISTA
            return RedirectToAction("Index");
        }
    }
}