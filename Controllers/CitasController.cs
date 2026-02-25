using Microsoft.AspNetCore.Mvc;
using ConsultorioVerde.Web.Models;

namespace ConsultorioVerde.Web.Controllers
{
    public class CitasController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        // Inyectamos el servicio en el constructor
        public CitasController(ApiServiceProxy apiService)
        {
            _apiProxy = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Obtener Citas (usando el objeto vacío según tu CURL)
            var filtro = new { idCita = 0, activo = true };
            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, filtro);

            // 2. Obtener Pacientes y Médicos para cruzar nombres
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { });
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { });

            // 3. Cruzar la información (Mapeo)
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

        public IActionResult Programar()
        {
            return View();
        }
    }
}