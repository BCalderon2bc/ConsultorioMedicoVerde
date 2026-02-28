    using System.ComponentModel.DataAnnotations;


namespace ConsultorioMedicoVerde.Models
{
    public class UsuarioViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string NombreUsuario { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un Rol")]
        public string Rol { get; set; }

        public int? IdMedico { get; set; }

        // Campos de auditoría
        public DateTime? FechaCreacion { get; set; }

        public string UsuarioCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public string UsuarioModificacion { get; set; }

        public bool Activo { get; set; }


        // Propiedades de control para la lógica del login
        public string Usuario { get; set; }
            
        public string Contrasena { get; set; }

    }


}
