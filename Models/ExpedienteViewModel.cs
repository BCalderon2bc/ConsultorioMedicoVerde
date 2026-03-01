using ConsultorioMedicoVerde.Models;

namespace ConsultorioVerde.Web.Models
{
    public class ExpedienteViewModel
    {
        public PacienteViewModel Paciente { get; set; }
        public List<HistorialMedicoViewModel> Historial { get; set; }
        public List<CitaViewModel> Citas { get; set; }
        public List<ConsultaViewModel> Consultas { get; set; }
        public List<RecetaViewModel> Recetas { get; set; }

    }
}