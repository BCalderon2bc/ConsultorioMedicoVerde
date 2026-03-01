using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ConsultorioVerde.Web.Models
{
    public class PacienteViewModel
    {
        [JsonPropertyName("idPaciente")]
        public int IdPaciente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [JsonPropertyName("nombre")]                                            
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        [JsonPropertyName("apellido")]
        public string Apellido { get; set; }

        [JsonIgnore] // Evita conflictos de serialización
        [Display(Name = "Paciente")]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        [JsonPropertyName("fechaNacimiento")]
        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [JsonPropertyName("genero")]
        [Display(Name = "Género")]
        public string? Genero { get; set; } // SQL: char(1)

        [JsonPropertyName("identificacion")] // Nombre exacto según tu última imagen
        [Display(Name = "Identificación")]
        [StringLength(16)]
        public string? Identificacion { get; set; }

        [JsonPropertyName("telefono")]
        [Display(Name = "Teléfono")]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [JsonPropertyName("correo")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido")]
        [Display(Name = "Correo Electrónico")]
        [StringLength(100)]
        public string? Correo { get; set; }

        [JsonPropertyName("direccion")]
        [Display(Name = "Dirección")]
        [StringLength(250)]
        public string? Direccion { get; set; }

        // --- CAMPOS DE AUDITORÍA ---
        [JsonPropertyName("fechaRegistro")]
        public DateTime? FechaRegistro { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime? FechaCreacion { get; set; }

        [JsonPropertyName("usuarioCreacion")]
        public string? UsuarioCreacion { get; set; }

        [JsonPropertyName("fechaModificacion")]
        public DateTime? FechaModificacion { get; set; }

        [JsonPropertyName("usuarioModificacion")]
        public string? UsuarioModificacion { get; set; }

        [JsonPropertyName("activo")]
        [Display(Name = "Estado")]
        public bool? Activo { get; set; } = false; // bit en SQL

        // Datos del Historial (Campos de tu nueva tabla)
        public string? Alergias { get; set; }
        public string? EnfermedadesPrevias { get; set; }
        public string? CirugiasPrevias { get; set; }
        public string? Observaciones { get; set; }
    }
}