using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly ApiServiceProxy _apiProxy;

    // Inyectamos el servicio en el constructor
    public AccountController(ApiServiceProxy apiService)
    {
        _apiProxy = apiService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            // Enviamos el objeto con "usuario" y "contrasena" como pide tu curl
            var usuarioValido = await _apiProxy.SendRequestAsync<UsuarioViewModel>(
                "Usuario", "ValidarLogin", HttpMethod.Post, model);

            if (usuarioValido != null && usuarioValido.Usuario != null)
            {
                var claims = new List<Claim>
                { 
                    new Claim(ClaimTypes.Name, usuarioValido.Usuario),
                    new Claim(ClaimTypes.Role, usuarioValido.Rol), // <--- AQUÍ GUARDAS EL ROL
                    new Claim("IdUsuario", usuarioValido.IdUsuario.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error de conexión: " + ex.Message);
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}