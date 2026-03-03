using ConsultorioMedicoVerde.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConsultorioMedicoVerde.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

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
                // Enviamos el objeto con "usuario" y "contrasena" al API
                var usuarioValido = await _apiProxy.SendRequestAsync<UsuarioViewModel>("Usuario", "ValidarLogin", HttpMethod.Post, model);

                if (usuarioValido != null && !string.IsNullOrEmpty(usuarioValido.Usuario))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuarioValido.Usuario),
                        new Claim(ClaimTypes.Role, usuarioValido.Rol ?? "Usuario"),
                        new Claim("IdUsuario", usuarioValido.IdUsuario.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties { IsPersistent = true });

                    return RedirectToAction("Index", "Home");
                }

                // Error de credenciales: Se mantiene en ModelState para que aparezca en el resumen de validación
                ModelState.AddModelError("", "El nombre de usuario o la contraseña son incorrectos.");
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "No se pudo establecer conexión con el servidor de autenticación.";
                ModelState.AddModelError("", "Servicio de API no disponible.");
            }
            catch (Exception)
            {
                TempData["Error"] = "Se produjo un fallo inesperado durante el inicio de sesión.";
                ModelState.AddModelError("", "Error interno del sistema.");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}