using Microsoft.AspNetCore.Mvc;
using ConsultorioVerde.Web.Models;
using System.Threading.Tasks;

namespace ConsultorioVerde.Web.Controllers
{   
    public class PacientesController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        // Inyectamos el servicio en el constructor
        public PacientesController(ApiServiceProxy apiService)
        {
            _apiProxy = apiService;
        }

        // GET: Pacientes
        public async Task<IActionResult> Index()
        {
            // Llamada dinámica indicando POST
            var lista = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post);

            return View(lista);
        }

        // GET: Pacientes/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Pacientes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PacienteViewModel paciente)
        {
            if (ModelState.IsValid)
            {
                // Aquí llamarías a tu servicio para POST (puedes implementarlo luego)
                // bool guardado = await _pacienteService.CrearPacienteAsync(paciente);

                return RedirectToAction(nameof(Index));
            }
            return View(paciente);
        }

        // GET: Pacientes/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            // Usamos el Proxy para obtener la lista y buscamos al paciente por ID
            // O si tu API tiene un método "ObtenerPaciente", úsalo.
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post);
            var paciente = pacientes.FirstOrDefault(p => p.IdPaciente == id);

            if (paciente == null)
            {
                return NotFound();
            }

            return View(paciente);
        }

        // POST: Pacientes/Editar/5
        [HttpPost]
        public async Task<IActionResult> Editar(PacienteViewModel paciente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Aquí mandas a llamar a tu método de actualizar en la API
                    await _apiProxy.SendRequestAsync<bool>("Paciente", "ActualizarPaciente", HttpMethod.Post, paciente);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "No se pudo actualizar: " + ex.Message);
                }
            }
            return View(paciente);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarLogico(int id)
        {
            try
            {
                // 1. Obtener los datos actuales del paciente
                var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post);
                var paciente = pacientes.FirstOrDefault(p => p.IdPaciente == id);

                if (paciente != null)
                {
                    // 2. Cambiar estado a inactivo
                    paciente.Activo = false;
                    paciente.FechaModificacion = DateTime.Now;
                    paciente.UsuarioModificacion = "Sistema"; // O el usuario logueado

                    // 3. Enviar actualización a la API
                    await _apiProxy.SendRequestAsync<bool>("Paciente", "ActualizarPaciente", HttpMethod.Post, paciente);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Puedes manejar el error aquí (ej. TempData["Error"] = "...")
                return RedirectToAction(nameof(Index));
            }
        }




    }
}