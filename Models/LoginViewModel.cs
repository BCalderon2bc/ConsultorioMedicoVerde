using System.ComponentModel.DataAnnotations;

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

public class UsuarioViewModel
{   

    public string Usuario { get; set; }
    public string Contrasena { get; set; }
    public bool Exito { get; set; }
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; }
    public string Rol { get; set; }
    public int? IdMedico { get; set; }
    public bool Activo { get; set; }
}