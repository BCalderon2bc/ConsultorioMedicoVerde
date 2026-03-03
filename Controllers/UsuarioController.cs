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
            ViewBag.MostrarActivos = mostrarActivos;
            ViewBag.Buscar = buscar;
            ViewBag.Filtro = filtro;

            var busqueda = new UsuarioViewModel { IdUsuario = 0 };

            if (!string.IsNullOrEmpty(buscar))
            {
                if (filtro == "Nombre")
                    busqueda.NombreUsuario = buscar;
                else if (filtro == "Rol")
                    busqueda.Rol = buscar;
            }

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

        ///Al entrar a pantalla de editar usuario se activa 
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
            var busqueda = new UsuarioViewModel { IdUsuario = id };

            var lista = await _apiProxy.SendRequestAsync<List<UsuarioViewModel>>("Usuario", "ListarUsuario", HttpMethod.Post, busqueda);

            var usuario = lista?.Where(e => e.IdUsuario == id);

            if (usuario == null || !usuario.Any())
            {
                TempData["Error"] = "El usuario solicitado no existe o no pudo ser localizado.";
                return RedirectToAction("Index");
            }

            await CargarMedicos();

            var userEdit = usuario.First();
            userEdit.Usuario = userEdit.NombreUsuario;
            userEdit.Contrasena = userEdit.Password;
            userEdit.UsuarioModificacion = usuarioLogueado;
            userEdit.FechaModificacion = DateTime.Now;

            return View("Formulario", userEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarEdicion(UsuarioViewModel model)
        {
            var idUsuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

            model.UsuarioModificacion = idUsuarioLogueado;
            model.FechaModificacion = DateTime.Now;
            model.Usuario = model.NombreUsuario;
            model.Contrasena = model.Password;

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("password", "La contraseña es obligatoria.");
                return View("Formulario", model);
            }

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
                TempData["MensajeExito"] = $"Datos de '{model.NombreUsuario}' actualizados exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "No se pudo completar la actualización del usuario. Verifique los datos.";
            await CargarMedicos();
            return View("Formulario", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(UsuarioViewModel model)
        {
            var idUsuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

            model.UsuarioCreacion = idUsuarioLogueado;
            model.FechaCreacion = DateTime.Now;
            model.Usuario = model.NombreUsuario;
            model.Contrasena = model.Password;

            if (model.IdUsuario == 0 && string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("password", "Es necesario asignar una contraseña al nuevo usuario.");
            }

            if (!ModelState.IsValid)
            {
                await CargarMedicos();
                return View("Formulario", model);
            }

            var busqueda = new UsuarioViewModel
            {
                NombreUsuario = model.NombreUsuario,
                Password = model.Password,
                Rol = model.Rol,
                IdMedico = model.IdMedico,
                FechaCreacion = model.FechaCreacion,
                UsuarioCreacion = model.UsuarioCreacion,
                Usuario = model.Usuario,
                Activo = model.Activo,
                Contrasena = model.Contrasena
            };

            var respuesta = await _apiProxy.SendRequestAsync<object>("Usuario", "InsertarUsuario", HttpMethod.Post, busqueda);

            if (respuesta != null)
            {
                TempData["MensajeExito"] = $"Usuario '{model.NombreUsuario}' creado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Ocurrió un error al procesar el nuevo usuario en el servidor.";
            await CargarMedicos();
            return View("Formulario", model);
        }

        private async Task CargarMedicos()
        {
            try
            {
                var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post);

                if (medicos != null)
                {
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
                var usuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                var usuarios = await _apiProxy.SendRequestAsync<List<UsuarioViewModel>>("Usuario", "ListarUsuario", HttpMethod.Post);
                var usuario = usuarios?.FirstOrDefault(p => p.IdUsuario == id);

                if (usuario == null)
                {
                    TempData["Error"] = "Error al intentar localizar el usuario seleccionado.";
                    return RedirectToAction("Index");
                }

                usuario.Activo = activo;
                usuario.UsuarioModificacion = usuarioLogueado;
                usuario.FechaModificacion = DateTime.Now;

                var resultado = await _apiProxy.SendRequestAsync<object>("Usuario", "ActualizarEstadoUsuario", HttpMethod.Put, usuario);

                TempData["MensajeExito"] = activo
                    ? $"El acceso de '{usuario.NombreUsuario}' ha sido habilitado."
                    : $"El acceso de '{usuario.NombreUsuario}' ha sido suspendido correctamente.";

                return RedirectToAction("Index", new { buscar, filtro, mostrarActivos });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Fallo crítico al actualizar estado: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}