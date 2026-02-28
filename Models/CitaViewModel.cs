using System.ComponentModel.DataAnnotations;

namespace ConsultorioVerde.Web.Models
{
    public class CitaViewModel
    {
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public string? NombrePaciente { get; set; } // Para mostrar en la tabla
        public int IdMedico { get; set; }
        public string? NombreMedico { get; set; }   
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaCita { get; set; }
        public string? Motivo { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public string? usuarioModificacion { get; set; }
    }
}