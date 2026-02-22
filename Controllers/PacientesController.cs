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
                try
                {
                    // Preparación del objeto según el JSON del CURL
                    paciente.IdPaciente = 0;
                    paciente.Activo = true;

                    DateTime ahora = DateTime.Now;
                    paciente.FechaCreacion = ahora;
                    paciente.FechaModificacion = ahora;
                    paciente.UsuarioCreacion = "WebUser";
                    paciente.UsuarioModificacion = "WebUser";

                    // Enviamos a la API
                    var respuesta = await _apiProxy.SendRequestAsync<object>("Paciente", "InsertarPaciente", HttpMethod.Post, paciente);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"Paciente {paciente.NombreCompleto} registrado correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al conectar con la API: " + ex.Message);
                }
            }
            return View(paciente);
        }

        // GET: Pacientes/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            // Buscamos al paciente. Nota: Usamos el listar y filtramos por ID
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post);
            var paciente = pacientes?.FirstOrDefault(p => p.IdPaciente == id);

            if (paciente == null) return NotFound();

            return View(paciente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(PacienteViewModel paciente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizamos datos de auditoría
                    paciente.FechaModificacion = DateTime.Now;
                    paciente.UsuarioModificacion = "WebUser";

                    // Importante: Tu API usa PUT según el curl
                    var respuesta = await _apiProxy.SendRequestAsync<object>("Paciente", "ActualizarPaciente", HttpMethod.Put, paciente);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"El paciente {paciente.Nombre} {paciente.Apellido} ha sido actualizado.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el paciente: " + ex.Message);
                }
            }
            return View(paciente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarLogico(int id)
        {
            try
            {
                // 1. Obtenemos la lista para encontrar al paciente por ID
                // Nota: Si tienes un endpoint 'ObtenerPacientePorId', sería más eficiente usar ese.
                var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post);
                var paciente = pacientes?.FirstOrDefault(p => p.IdPaciente == id);

                if (paciente != null)
                {
                    // 2. Modificamos el estado y la auditoría
                    paciente.Activo = false;
                    paciente.FechaModificacion = DateTime.Now;
                    paciente.UsuarioModificacion = "WebUser";

                    // 3. Enviamos la actualización mediante PUT
                    var respuesta = await _apiProxy.SendRequestAsync<object>("Paciente", "ActualizarEstadoPaciente", HttpMethod.Put, paciente);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"El paciente {paciente.NombreCompleto} ha sido desactivado.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al intentar desactivar el registro: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }





    }
}