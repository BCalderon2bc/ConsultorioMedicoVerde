using Microsoft.AspNetCore.Mvc;
using ConsultorioVerde.Web.Models;

namespace ConsultorioVerde.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory) 
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // COMENTADO TEMPORALMENTE PARA AVANZAR EN EL DISEÑO
            /*
            var client = _httpClientFactory.CreateClient("ConsultorioAPI");
            var loginData = new { Username = model.Email, Password = model.Password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("auth/login", content);

            if (response.IsSuccessStatusCode) { ... }
            */

            // SIMULACIÓN DE LOGIN EXITOSO:
            if (model.Email == "admin" && model.Password == "123") // Solo para tus pruebas locales
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }
    }
}