namespace ConsultorioVerde.Web.Models
{
    public class CitaViewModel
    {
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public string NombrePaciente { get; set; } // Para mostrar en la tabla
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraCita { get; set; }
        public string Motivo { get; set; }
        public string Estado { get; set; } // Ejemplo: Pendiente, Completada, Cancelada
    }
}