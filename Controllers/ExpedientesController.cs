using Microsoft.AspNetCore.Mvc;
using ConsultorioVerde.Web.Models;

namespace ConsultorioVerde.Web.Controllers
{
    public class ExpedientesController : Controller
    {
        public IActionResult Index()
        {
            // Datos de prueba: Historial clínico de un paciente
            var historial = new List<ExpedienteViewModel>
            {
                new ExpedienteViewModel {
                    IdExpediente = 101,
                    NombrePaciente = "Juan Pérez",
                    FechaConsulta = new DateTime(2023, 10, 05),
                    MotivoConsulta = "Dolor lumbar crónico",
                    Diagnostico = "Lumbalgia mecánica",
                    Tratamiento = "10 sesiones de electroterapia y masajes",
                    Observaciones = "Paciente presenta mejoría en movilidad."
                }
            };
            return View(historial);
        }

        public IActionResult NuevaEntrada(int idPaciente)
        {
            return View();
        }
    }
}