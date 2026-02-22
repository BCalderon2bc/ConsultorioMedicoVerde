using Microsoft.AspNetCore.Mvc;
using ConsultorioVerde.Web.Models;

namespace ConsultorioVerde.Web.Controllers
{
    public class CitasController : Controller
    {
        public IActionResult Index()
        {
            // Datos de prueba que coinciden con tu lógica de clínica de rehabilitación
            var citas = new List<CitaViewModel>
            {
                new CitaViewModel {
                    IdCita = 1,
                    NombrePaciente = "Juan Pérez",
                    FechaCita = DateTime.Today,
                    HoraCita = new TimeSpan(9, 0, 0),
                    Motivo = "Fisioterapia de Rodilla",
                    Estado = "Pendiente"
                },
                new CitaViewModel {
                    IdCita = 2,
                    NombrePaciente = "María López",
                    FechaCita = DateTime.Today,
                    HoraCita = new TimeSpan(10, 30, 0),
                    Motivo = "Masaje Terapéutico",
                    Estado = "Confirmada"
                }
            };

            return View(citas);
        }

        public IActionResult Programar()
        {
            return View();
        }
    }
}