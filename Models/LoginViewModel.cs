using System.ComponentModel.DataAnnotations;

namespace ConsultorioVerde.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo o usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}