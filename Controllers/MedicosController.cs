
using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConsultorioVerde.Web.Controllers
{
    public class MedicosController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        public MedicosController(ApiServiceProxy apiProxy)
        {
            _apiProxy = apiProxy;
        }

        public async Task<IActionResult> Index()
        {
            // Llamada al endpoint que me pasaste
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post);
            return View(medicos ?? new List<MedicoViewModel>());
        }

        // 1. GET: Muestra el formulario en blanco
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(MedicoViewModel medico)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Forzamos un formato de fecha que SQL y la API entiendan sin problemas
                    DateTime fechaLimpia = new DateTime(
                        DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                        DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second
                    );

                    medico.IdMedico = 0;
                    medico.Activo = true;
                    medico.FechaCreacion = fechaLimpia;
                    medico.FechaModificacion = fechaLimpia;
                    medico.UsuarioCreacion = "WebUser";
                    medico.UsuarioModificacion = "WebUser";

                    // En el envío, asegúrate de que el nombre del parámetro coincida 
                    // con lo que espera la API (según el error BadRequest)
                    var respuesta = await _apiProxy.SendRequestAsync<object>("Medico", "InsertarMedico", HttpMethod.Post, medico);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"Médico {medico.Nombre} registrado con éxito.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Si la API responde con 400, capturamos el detalle aquí
                    ModelState.AddModelError("", "La API rechazó los datos: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error inesperado: " + ex.Message);
                }
            }
            return View(medico);
        }


        // GET: Medicos/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            // Buscamos al médico en la lista actual de la API
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post);
            var medico = medicos?.FirstOrDefault(m => m.IdMedico == id);

            if (medico == null) return NotFound();

            return View(medico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(MedicoViewModel medico)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Mantenemos la auditoría
                    medico.FechaModificacion = DateTime.Now;
                    medico.UsuarioModificacion = "WebUser";

                    // ¡IMPORTANTE! Usamos HttpMethod.Put según tu curl
                    var respuesta = await _apiProxy.SendRequestAsync<object>("Medico", "ActualizarMedico", HttpMethod.Put, medico);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"Los datos de {medico.Nombre} se actualizaron correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                }
            }
            return View(medico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarLogico(int id)
        {
            try
            {
                // 1. Buscamos al médico actual para tener todos sus datos (la API pide el objeto completo)
                var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post);
                var medico = medicos?.FirstOrDefault(m => m.IdMedico == id);

                if (medico != null)
                {
                    // 2. Cambiamos solo el estado y la auditoría
                    medico.Activo = false;
                    medico.FechaModificacion = DateTime.Now;
                    medico.UsuarioModificacion = "WebUser";

                    // 3. Enviamos el PUT al endpoint especial
                    var respuesta = await _apiProxy.SendRequestAsync<object>("Medico", "ActualizarEstadoMedico", HttpMethod.Put, medico);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = "El médico ha sido desactivado correctamente.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al desactivar: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}