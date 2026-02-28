using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConsultorioVerde.Web.Controllers
{
    public class CitasController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        public CitasController(ApiServiceProxy apiService)
        {
            _apiProxy = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Obtener Citas (usando el objeto vacío según tu CURL)
            var filtro = new { idCita = 0, activo = true };
            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, filtro);

            // 2. Obtener Pacientes para cruzar nombres
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { });

            // 3. Obtener Médicos para cruzar nombres
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { });

            // 4. Cruzar la información (Mapeo)
            if (citas != null)
            {
                foreach (var cita in citas)
                {
                    cita.NombrePaciente = pacientes?.FirstOrDefault(p => p.IdPaciente == cita.IdPaciente)?.NombreCompleto ?? "Desconocido";
                    cita.NombreMedico = medicos?.FirstOrDefault(m => m.IdMedico == cita.IdMedico)?.NombreCompleto ?? "Desconocido";
                }
            }

            return View(citas ?? new List<CitaViewModel>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CitaViewModel citaMedica)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var idUsuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                    citaMedica.FechaCreacion = DateTime.Now;
                    citaMedica.UsuarioCreacion = idUsuarioLogueado;
                    citaMedica.Activo = true;

                    // En el envío, asegúrate de que el nombre del parámetro coincida 
                    // con lo que espera la API (según el error BadRequest)
                    var respuesta = await _apiProxy.SendRequestAsync<object>("CitaMedica", "InsertarCitaMedica", HttpMethod.Put, citaMedica);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"Cita médica registrada con éxito.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Si la API responde con 400, capturamos el detalle aquí
                    ModelState.AddModelError("", "La API rechazó los datos: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error inesperado: " + ex.Message);
                }
            }
            return View(citaMedica);
        }

        public async Task<IActionResult> Programar()
        {
            // Obtener Pacientes
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { idPaciente = 0 });
            // Obtener Médicos
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { idMedico = 0 });

            // IMPORTANTE: Asegurar que no sean null para que la vista no explote
            ViewBag.Pacientes = pacientes ?? new List<PacienteViewModel>();
            ViewBag.Medicos = medicos ?? new List<MedicoViewModel>();

            return View(new CitaViewModel { FechaCita = DateTime.Now });
        }
    }
}