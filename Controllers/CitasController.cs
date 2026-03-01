using ConsultorioMedicoVerde.Models;
using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConsultorioVerde.Web.Controllers
{
    public class RespuestaConsulta
    {
        public int idConsulta { get; set; }
    }
    public class CitasController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        public CitasController(ApiServiceProxy apiService)
        {
            _apiProxy = apiService;
        }

        public async Task<IActionResult> Index(string buscar, string filtro, DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Si no vienen fechas, usamos DateTime.Today (media noche de hoy)
            DateTime fInicio = fechaInicio ?? DateTime.Today;
            DateTime fFin = fechaFin ?? DateTime.Today;

            if (fechaFin == null)
                ViewBag.FechaFin = DateTime.Today.ToString("yyyy-MM-dd");
            else
                ViewBag.FechaFin = fechaFin;

            // 1. Configurar el objeto de búsqueda con valores por defecto o seleccionados
            var busqueda = new CitaViewModel
            {
                IdCita = 0,
                NombreMedico = null,
                NombrePaciente = null,
                Estado = null,
                // Si el usuario no elige fecha, usamos un rango muy amplio
                FechaInicio = fechaInicio ?? DateTime.Now,
                FechaFin = fechaFin ?? DateTime.Now,
                FechaCita = DateTime.Now,

            };

            // 2. Mapear el texto de búsqueda según el filtro seleccionado
            if (!string.IsNullOrEmpty(buscar))
            {
                if (filtro == "Nombre Médico")
                    busqueda.NombreMedico = buscar;
                else if (filtro == "Nombre Paciente")
                    busqueda.NombrePaciente = buscar;
                else if (filtro == "Estado")
                    busqueda.Estado = buscar;
            }

            // 3. Consumir el API de ListarCitaMedica con el objeto filtrado
            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, busqueda);

            // 4. Obtener catálogos para cruzar nombres (Pacientes y Médicos)
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { });
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { });

            // 5. Cruzar la información (Mapeo de Nombres)
            if (citas != null)
            {
                foreach (var cita in citas)
                {
                    var p = pacientes?.FirstOrDefault(x => x.IdPaciente == cita.IdPaciente);
                    var m = medicos?.FirstOrDefault(x => x.IdMedico == cita.IdMedico);

                    cita.NombrePaciente = p != null ? $"{p.Nombre} {p.Apellido}" : "Desconocido";
                    cita.NombreMedico = m != null ? $"{m.Nombre} {m.Apellido}" : "Desconocido";
                }
            }

            // 6. Persistencia para que los filtros se mantengan visibles en la vista
            ViewBag.Buscar = buscar;
            ViewBag.Filtro = filtro;

            ViewBag.FechaInicio = fInicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fFin.ToString("yyyy-MM-dd");

            // Re-mapear NombreConCedula como hicimos antes
            ViewBag.Pacientes = pacientes?.Select(p => new {
                p.IdPaciente,
                NombreConCedula = $"{p.Nombre} {p.Apellido} - {p.Identificacion}"
            }).ToList();

            // 3. Preparar lista de Médicos: "Nombre Apellido - Especialidad"
            ViewBag.Medicos = medicos?.Select(m => new {
                IdMedico = m.IdMedico,
                // Aquí puedes agregar la especialidad si la tienes en el modelo para diferenciar médicos
                NombreConEspecialidad = $"{m.Nombre} {m.Apellido} {(string.IsNullOrEmpty(m.NumeroRegistroMINSA) ? "" : "- " + m.NumeroRegistroMINSA)}"
            }).ToList();

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
            var filtro = new { idCita = id};
            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, filtro);
            var cita = citas.Where((e) => e.IdCita == id).ToList();

            // 2. Cargar Listas para los Selects
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { });
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { });


            // Re-mapear NombreConCedula como hicimos antes
            ViewBag.Pacientes = pacientes?.Select(p => new {
                p.IdPaciente,
                NombreConCedula = $"{p.Nombre} {p.Apellido} - {p.Identificacion}"
            }).ToList();

            // 3. Preparar lista de Médicos: "Nombre Apellido - Especialidad"
            ViewBag.Medicos = medicos?.Select(m => new {
                IdMedico = m.IdMedico,
                // Aquí puedes agregar la especialidad si la tienes en el modelo para diferenciar médicos
                NombreConEspecialidad = $"{m.Nombre} {m.Apellido} {(string.IsNullOrEmpty(m.NumeroRegistroMINSA) ? "" : "- " + m.NumeroRegistroMINSA)}"
            }).ToList();

            return View(cita.First());
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

            // 2. Preparar lista de Pacientes: "Nombre Apellido - Cédula"
            ViewBag.Pacientes = pacientes?.Select(p => new {
                IdPaciente = p.IdPaciente,
                NombreCompleto = $"{p.Nombre} {p.Apellido} - {p.Identificacion}"
            }).ToList();

            // 3. Preparar lista de Médicos: "Nombre Apellido - Especialidad"
            ViewBag.Medicos = medicos?.Select(m => new {
                IdMedico = m.IdMedico,
                // Aquí puedes agregar la especialidad si la tienes en el modelo para diferenciar médicos
                NombreConEspecialidad = $"{m.Nombre} {m.Apellido} {(string.IsNullOrEmpty(m.NumeroRegistroMINSA) ? "" : "- " + m.NumeroRegistroMINSA)}"
            }).ToList();



            return View(new CitaViewModel { FechaCita = DateTime.Now });
        }


        ///Consultas medicas

        // Asegúrate de que el nombre sea "Atender" y reciba el "id"
        public async Task<IActionResult> Atender(int id)
        {
            if (id == 0) return RedirectToAction("Index");

            // 1. Obtener todas las citas
            var filtro = new { idCita = id};
            List<CitaViewModel> citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, filtro);

            var cita = citas.Where((e)=>e.IdCita == id).ToList();
            if (!cita.Any()) return NotFound();

            // 2. Mapear al modelo de la vista de Consulta
            var consulta = new ConsultaViewModel
            {
                IdCita = cita.First().IdCita,
                NombrePaciente = cita.First().NombrePaciente,
                NombreMedico = cita.First().NombreMedico,
                MotivoCita = cita.First().Motivo,
                FechaConsulta = DateTime.Now,
            };

            return View(consulta); // Esto busca la vista Views/Citas/Atender.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConsulta(ConsultaViewModel modelo)
        {
            
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Objeto para [InsertarConsulta] según tu CURL
                    var consultaApi = new
                    {
                        idCita = modelo.IdCita,
                        fechaConsulta = DateTime.Now,
                        diagnostico = modelo.Diagnostico,
                        tratamiento = modelo.Tratamiento,
                        notas = modelo.Notas,
                        fechaCreacion = DateTime.Now,
                        usuarioCreacion = User.Identity?.Name ?? "Medico_User",
                        fechaModificacion = DateTime.Now,
                        usuarioModificacion = User.Identity?.Name ?? "Medico_User"
                    };

                    // 2. Ejecutar Inserción de la Consulta
                    var respuesta = await _apiProxy.SendRequestAsync<RespuestaConsulta>(
                        "Consulta",
                        "InsertarConsulta",
                        HttpMethod.Post,
                        consultaApi);

                    if (respuesta != null)
                    {
                        // 2. Aquí ya puedes obtener el 8 directamente
                        int idGenerado = respuesta.idConsulta;

                        // 3. PERSISTENCIA: Pasamos el ID al Index para el Modal
                        TempData["ConsultaGuardadaId"] = idGenerado;
                        TempData["MensajeExito"] = "Consulta registrada correctamente.";

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al procesar la consulta: " + ex.Message);
                }
            }

            // Si llegamos aquí, algo falló, recargamos la vista de Atender
            return View("Atender", modelo);
        }


        // GET: Citas/CrearReceta?idConsulta=5
        public IActionResult CrearReceta(int idConsulta)
        {
            return View(new RecetaViewModel { IdConsulta = idConsulta });
        }

        // POST: Citas/GuardarReceta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarReceta(RecetaViewModel modelo)
        {
            try
            {
                // Objeto exacto según tu CURL de RecetaMedica
                var recetaApi = new
                {
                   // idReceta = 0,
                    idConsulta = modelo.IdConsulta,
                    medicamento = modelo.Medicamento,
                    dosis = modelo.Dosis,
                    frecuencia = modelo.Frecuencia,
                    duracion = modelo.Duracion,
                   // fechaCreacion = DateTime.Now,
                    usuarioCreacion = User.Identity?.Name ?? "Medico_User",
                   // fechaModificacion = DateTime.Now,
                   // usuarioModificacion = User.Identity?.Name ?? "Medico_User"
                };

                var respuesta = await _apiProxy.SendRequestAsync<object>("RecetaMedica", "InsertarRecetaMedica", HttpMethod.Post, recetaApi);

                if (respuesta != null)
                {
                    // Guardamos el IdConsulta para que el modal sepa a qué consulta volver si quiere agregar otro
                    TempData["RecetaGuardadaIdConsulta"] = modelo.IdConsulta;
                    TempData["MensajeExito"] = "Medicamento agregado correctamente.";

                    // Redirigimos a la misma pantalla (GET) para procesar el modal
                    return RedirectToAction("CrearReceta", new { idConsulta = modelo.IdConsulta });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar medicamento: " + ex.Message);
            }

            return View(modelo);
        }

    }
}