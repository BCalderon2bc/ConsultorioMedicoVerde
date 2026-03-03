using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConsultorioVerde.Web.Controllers
{
    public class MedicosController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        public MedicosController(ApiServiceProxy apiProxy)
        {
            _apiProxy = apiProxy;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string buscar, string filtro)
        {
            // Preparar objeto para el API
            var busqueda = new MedicoViewModel
            {
                IdMedico = 0,
                Nombre = "",
                Apellido = "",
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now,
            };

            if (!string.IsNullOrEmpty(buscar))
            {
                if (filtro == "Nombre")
                    busqueda.Nombre = buscar;
                else if (filtro == "Apellido")
                    busqueda.Apellido = buscar;
            }

            // Consumir el API de ListarMedico
            var lista = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, busqueda);

            // Persistencia para la vista
            ViewBag.Buscar = buscar;
            ViewBag.Filtro = filtro;

            return View(lista ?? new List<MedicoViewModel>());
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(MedicoViewModel medico)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                    // Formato de fecha limpio para evitar conflictos con SQL/API
                    DateTime fechaActual = DateTime.Now;

                    medico.IdMedico = 0;
                    medico.Activo = true;
                    medico.FechaCreacion = fechaActual;
                    medico.FechaModificacion = fechaActual;
                    medico.UsuarioCreacion = usuarioLogueado;
                    medico.UsuarioModificacion = usuarioLogueado;

                    var respuesta = await _apiProxy.SendRequestAsync<object>("Medico", "InsertarMedico", HttpMethod.Post, medico);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"El Dr. {medico.Nombre} {medico.Apellido} ha sido registrado exitosamente en el sistema.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (HttpRequestException ex)
                {
                    TempData["Error"] = "Los datos enviados no cumplen con el formato requerido por el servidor.";
                    ModelState.AddModelError("", "Error de validación en API: " + ex.Message);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Ocurrió un fallo inesperado al intentar registrar al médico.";
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }
            return View(medico);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post);
            var medico = medicos?.FirstOrDefault(m => m.IdMedico == id);

            if (medico == null)
            {
                TempData["Error"] = "No se encontró el registro del médico solicitado.";
                return RedirectToAction(nameof(Index));
            }

            return View(medico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(MedicoViewModel medico)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                    medico.FechaModificacion = DateTime.Now;
                    medico.UsuarioModificacion = usuarioLogueado;

                    var respuesta = await _apiProxy.SendRequestAsync<object>("Medico", "ActualizarMedico", HttpMethod.Put, medico);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"La información del Dr. {medico.Nombre} ha sido actualizada correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "No se pudieron guardar los cambios realizados.";
                    ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                }
            }
            return View(medico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarLogico(int id, bool activo)
        {
            try
            {
                var usuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post);
                var medico = medicos?.FirstOrDefault(m => m.IdMedico == id);

                if (medico != null)
                {
                    medico.Activo = activo;
                    medico.FechaModificacion = DateTime.Now;
                    medico.UsuarioModificacion = usuarioLogueado;

                    var respuesta = await _apiProxy.SendRequestAsync<object>("Medico", "ActualizarEstadoMedico", HttpMethod.Put, medico);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = activo
                            ? $"El Dr. {medico.Nombre} ha sido reactivado en el sistema."
                            : $"El Dr. {medico.Nombre} ha sido desactivado de la agenda médica.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error crítico al intentar cambiar el estado del médico: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}