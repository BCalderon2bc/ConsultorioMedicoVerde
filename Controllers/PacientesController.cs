using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConsultorioVerde.Web.Controllers
{
    public class RespuestaConsultaPaciente
    {
        public int idPaciente { get; set; }
    }

    public class PacientesController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        public PacientesController(ApiServiceProxy apiService)
        {
            _apiProxy = apiService;
        }

        // GET: Pacientes
        public async Task<IActionResult> Index(string buscar, string filtro)
        {
            var busqueda = new PacienteViewModel
            {
                IdPaciente = 0,
                Nombre = "",
                Apellido = "",
                Identificacion = "",
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now,
            };

            if (!string.IsNullOrEmpty(buscar))
            {
                if (filtro == "Nombre") busqueda.Nombre = buscar;
                else if (filtro == "Apellido") busqueda.Apellido = buscar;
                else if (filtro == "Identificacion") busqueda.Identificacion = buscar;
            }

            ViewBag.Buscar = buscar;
            ViewBag.Filtro = filtro;

            var lista = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, busqueda);

            return View(lista ?? new List<PacienteViewModel>());
        }

        // GET: Pacientes/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Pacientes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PacienteViewModel paciente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var idUsuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                    paciente.Activo = true;
                    paciente.FechaCreacion = DateTime.Now;
                    paciente.UsuarioCreacion = idUsuarioLogueado;

                    var respuesta = await _apiProxy.SendRequestAsync<RespuestaConsultaPaciente>("Paciente", "InsertarPaciente", HttpMethod.Post, paciente);

                    if (respuesta != null)
                    {
                        int idNuevoPaciente = respuesta.idPaciente;

                        // Verificamos si hay datos de historial para insertar
                        if (!string.IsNullOrWhiteSpace(paciente.Alergias)
                            || !string.IsNullOrWhiteSpace(paciente.EnfermedadesPrevias)
                            || !string.IsNullOrWhiteSpace(paciente.CirugiasPrevias)
                            || !string.IsNullOrWhiteSpace(paciente.Observaciones))
                        {
                            var historialApi = new
                            {
                                idPaciente = idNuevoPaciente,
                                alergias = paciente.Alergias,
                                enfermedadesPrevias = paciente.EnfermedadesPrevias,
                                cirugiasPrevias = paciente.CirugiasPrevias,
                                observaciones = paciente.Observaciones,
                                fechaCreacion = DateTime.Now,
                                usuarioCreacion = idUsuarioLogueado
                            };

                            await _apiProxy.SendRequestAsync<object>("HistorialMedico", "InsertarHistorialMedico", HttpMethod.Post, historialApi);
                        }

                        TempData["MensajeExito"] = $"Paciente '{paciente.Nombre} {paciente.Apellido}' registrado exitosamente.";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Fallo de comunicación con el servidor al intentar crear el paciente.";
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(paciente);
        }

        // GET: Pacientes/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post);
            var paciente = pacientes?.FirstOrDefault(p => p.IdPaciente == id);

            if (paciente == null)
            {
                TempData["Error"] = "No se pudo encontrar el expediente del paciente seleccionado.";
                return RedirectToAction("Index");
            }

            return View(paciente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(PacienteViewModel paciente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var idUsuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                    paciente.FechaModificacion = DateTime.Now;
                    paciente.UsuarioModificacion = idUsuarioLogueado;

                    var respuesta = await _apiProxy.SendRequestAsync<object>("Paciente", "ActualizarPaciente", HttpMethod.Put, paciente);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"Los datos de '{paciente.Nombre} {paciente.Apellido}' han sido actualizados correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Ocurrió un error inesperado al actualizar el expediente.";
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(paciente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarLogico(int id, bool activo)
        {
            try
            {
                var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post);
                var paciente = pacientes?.FirstOrDefault(p => p.IdPaciente == id);

                if (paciente != null)
                {
                    var idUsuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                    paciente.Activo = activo;
                    paciente.FechaModificacion = DateTime.Now;
                    paciente.UsuarioModificacion = idUsuarioLogueado;

                    var respuesta = await _apiProxy.SendRequestAsync<object>("Paciente", "ActualizarEstadoPaciente", HttpMethod.Put, paciente);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = activo
                            ? "El paciente ha sido reactivado exitosamente."
                            : "El expediente del paciente ha sido desactivado correctamente.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo cambiar el estado del registro: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}