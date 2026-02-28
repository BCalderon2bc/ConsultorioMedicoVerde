using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConsultorioVerde.Web.Controllers
{
    public class CitasController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        public CitasController(ApiServiceProxy apiService)
        {
            _apiProxy = apiService;
        }

        public async Task<IActionResult> Index(string buscar, string filtro)
        {
            var busqueda = new CitaViewModel
            {
                NombreMedico = null,
                NombrePaciente = null,
                Estado = null,
                FechaInicio = DateTime.Parse("2020-02-27 14:00:00.000"),
                FechaFin = DateTime.Parse("2025-02-27 14:00:00.000"),
                FechaCita = DateTime.Now,

            };

            // var busqueda = new { idCita = 0, activo = true, fechaInicio = DateTime.Now };


            if (!string.IsNullOrEmpty(buscar))
            {
                if (filtro == "Nombre Médico")
                    busqueda.NombreMedico = buscar;
                else if (filtro == "Nombre Paciente")
                    busqueda.NombrePaciente = buscar;
                else if (filtro == "Estado")
                    busqueda.Estado = buscar;
            }

            // Consumir el API de ListarMedico
            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, busqueda);

            // Persistencia para la vista
            ViewBag.Buscar = buscar;
            ViewBag.Filtro = filtro;

//            return View(lista ?? new List<MedicoViewModel>());

            // 1. Obtener Citas (usando el objeto vacío según tu CURL)
          //  var filtro = new { idCita = 0, activo = true };
           // var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, filtro);

            // 2. Obtener Pacientes para cruzar nombres
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { });

            // 3. Obtener Médicos para cruzar nombres
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { });

            // 4. Cruzar la información (Mapeo)
            if (citas != null)
            {
                foreach (var cita in citas)
                {
                    cita.NombrePaciente = pacientes?.FirstOrDefault(p => p.IdPaciente == cita.IdPaciente)?.NombreCompleto ?? "Desconocido";
                    cita.NombreMedico = medicos?.FirstOrDefault(m => m.IdMedico == cita.IdMedico)?.NombreCompleto ?? "Desconocido";
                }
            }

            return View(citas ?? new List<CitaViewModel>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CitaViewModel citaMedica)
        {
            ModelState.Remove("NombreMedico");
            ModelState.Remove("NombrePaciente");
            ModelState.Remove("UsuarioCreacion");
            ModelState.Remove("usuarioModificacion");

            if (ModelState.IsValid)
            {
                try
                {
                    var idUsuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                    citaMedica.FechaCreacion = citaMedica.FechaCita;
                    citaMedica.UsuarioCreacion = idUsuarioLogueado;

                    // En el envío, asegúrate de que el nombre del parámetro coincida 
                    // con lo que espera la API (según el error BadRequest)
                    var respuesta = await _apiProxy.SendRequestAsync<object>("CitaMedica", "InsertarCitaMedica", HttpMethod.Post, citaMedica);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = $"Cita médica registrada con éxito.";
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
            return View(citaMedica);
        }

        // GET: Citas/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            // 1. Obtener la cita específica
            var filtro = new { idCita = id, activo = true };
            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, filtro);
            var cita = citas?.FirstOrDefault();

            if (cita == null) return NotFound();

            // 2. Cargar Listas para los Selects
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { });
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { });

            ViewBag.Pacientes = pacientes ?? new List<PacienteViewModel>();
            ViewBag.Medicos = medicos ?? new List<MedicoViewModel>();

            return View(cita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(CitaViewModel citaMedica)
        {          
            if (ModelState.IsValid)
            {
                try
                {
                    // Auditoría de Modificación
                    citaMedica.usuarioModificacion = User.Identity?.Name ?? "Sistema";
                    citaMedica.FechaModificacion = DateTime.Now;

                    // Según tu CURL, el endpoint de actualización es PUT
                    var respuesta = await _apiProxy.SendRequestAsync<object>("CitaMedica", "ActualizarCitaMedica", HttpMethod.Put, citaMedica);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = "Cita actualizada correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                }
            }

            // Si falla, recargar listas
            return await Editar(citaMedica.IdCita);
        }

        public async Task<IActionResult> Programar()
        {
            // Obtener Pacientes
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { idPaciente = 0 });
            // Obtener Médicos
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { idMedico = 0 });

            // IMPORTANTE: Asegurar que no sean null para que la vista no explote
            ViewBag.Pacientes = pacientes ?? new List<PacienteViewModel>();
            ViewBag.Medicos = medicos ?? new List<MedicoViewModel>();

            return View(new CitaViewModel { FechaCita = DateTime.Now });
        }
    }
}