using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ConsultorioVerde.Web.Models
{
    public class MedicoViewModel
    {
        [JsonPropertyName("idMedico")]
        public int IdMedico { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [JsonPropertyName("apellido")]
        public string Apellido { get; set; }

        // Propiedad calculada para mostrar en las tablas de la vista
        [JsonIgnore]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        [JsonPropertyName("especialidad")]
        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [JsonPropertyName("telefono")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [JsonPropertyName("correo")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido")]
        public string? Correo { get; set; }

        [JsonPropertyName("numeroRegistroMINSA")]
        [Display(Name = "Registro MINSA")]
        public string? NumeroRegistroMINSA { get; set; }

        [JsonPropertyName("activo")]
        public bool Activo { get; set; }

        // --- CAMPOS DE AUDITORÍA (Coinciden con tu JSON) ---

        [JsonPropertyName("fechaCreacion")]
        public DateTime? FechaCreacion { get; set; }

        [JsonPropertyName("usuarioCreacion")]
        public string? UsuarioCreacion { get; set; }

        [JsonPropertyName("fechaModificacion")]
        public DateTime? FechaModificacion { get; set; }

        [JsonPropertyName("usuarioModificacion")]
        public string? UsuarioModificacion { get; set; }
    }
}