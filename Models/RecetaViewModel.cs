using System;
using System.ComponentModel.DataAnnotations;

namespace ConsultorioMedicoVerde.Models
{
    public class RecetaViewModel
    {
        public int IdReceta { get; set; }

        [Required]
        public int IdConsulta { get; set; }

        [Required(ErrorMessage = "El nombre del medicamento es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Medicamento { get; set; }

        [StringLength(50)]
        public string Dosis { get; set; }

        [StringLength(50)]
        public string Frecuencia { get; set; }

        [StringLength(50)]
        public string Duracion { get; set; }

        // Campos de auditoría (opcionales para la vista, necesarios para el API)
        public DateTime? FechaCreacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioModificacion { get; set; }

        // Propiedades auxiliares para mostrar información en la vista de receta
        public string? NombrePaciente { get; set; }
        public string? DiagnosticoRelacionado { get; set; }
    }
}