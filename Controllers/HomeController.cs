using ConsultorioMedicoVerde.Models;
using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ConsultorioMedicoVerde.Controllers
{
    public class HomeController : Controller
    {
      
        private readonly ApiServiceProxy _apiProxy;

        public HomeController(ApiServiceProxy apiService)
        {
            _apiProxy = apiService;

        }

        public async Task<IActionResult> Index()
        {
            // 1. Obtenemos datos de ambos servicios
            var listaPacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post);
            var listaCitas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post);

            // 2. Filtramos las citas de hoy (comparando solo la parte de la fecha)
            var fechaHoy = DateTime.Today; // Esto es 05/03/2026 00:00:00

            // Usamos .Date para extraer solo la parte de la fecha de la base de datos
            var citasDeHoy = listaCitas.Where(c => (c.FechaCita ?? DateTime.Today).Date == fechaHoy) .OrderBy(c => c.FechaCita).ToList();

            // 3. Llenamos el modelo
            var model = new HomeViewModel
            {
                TotalPacientes = listaPacientes.Where(p=>p.Activo ?? false).ToList().Count,
                TotalCitasHoy = citasDeHoy.Count,
                ProximasCitas = citasDeHoy // Por si quieres mostrar la tablita en el Dashboard
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
