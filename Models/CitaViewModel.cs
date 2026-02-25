namespace ConsultorioVerde.Web.Models
{
    public class CitaViewModel
    {
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public string NombrePaciente { get; set; } // Para mostrar en la tabla
        public int IdMedico { get; set; }
        public string NombreMedico { get; set; }   // Para mostrar en la tabla
        public DateTime FechaCita { get; set; }
        public string Motivo { get; set; }
        public string Estado { get; set; }
        public bool Activo { get; set; }

        // Auditoría (según tu CREATE TABLE)
        public DateTime? FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; }
    }
}