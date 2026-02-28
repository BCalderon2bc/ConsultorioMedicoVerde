using System.ComponentModel.DataAnnotations;

namespace ConsultorioMedicoVerde.Models
{
    // Para el formulario de inicio de sesión
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]

        public string Contrasena { get; set; }

        public string Rol { get; set; } = "";
        public int? IdMedico { get; set; } = 0;

    }
}