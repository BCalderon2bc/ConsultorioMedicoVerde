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
            DateTime fInicio = fechaInicio ?? DateTime.Today;
            DateTime fFin = fechaFin ?? DateTime.Today;

            var busqueda = new CitaViewModel
            {
                IdCita = 0,
                NombreMedico = null,
                NombrePaciente = null,
                Estado = null,
                FechaInicio = fInicio,
                FechaFin = fFin,
                FechaCita = DateTime.Now,
            };

            if (!string.IsNullOrEmpty(buscar))
            {
                if (filtro == "Nombre Médico")
                    busqueda.NombreMedico = buscar;
                else if (filtro == "Nombre Paciente")
                    busqueda.NombrePaciente = buscar;
                else if (filtro == "Estado")
                    busqueda.Estado = buscar;
            }

            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, busqueda);

            // Cargamos catálogos para el Index y para cruzar nombres
            await CargarCatalogosCita();

            if (citas != null)
            {
                // Recuperamos las listas del ViewBag para el cruce de nombres
                var pacientesCruce = (IEnumerable<dynamic>)ViewBag.Pacientes;
                var medicosCruce = (IEnumerable<dynamic>)ViewBag.Medicos;

                foreach (var cita in citas)
                {
                    // Nota: Aquí buscamos en los objetos dinámicos creados en CargarCatalogosCita
                    var p = pacientesCruce?.FirstOrDefault(x => x.IdPaciente == cita.IdPaciente);
                    var m = medicosCruce?.FirstOrDefault(x => x.IdMedico == cita.IdMedico);

                    cita.NombrePaciente = p != null ? p.NombreConCedula : "Desconocido";
                    cita.NombreMedico = m != null ? m.NombreConEspecialidad : "Desconocido";
                }
            }

            ViewBag.Buscar = buscar;
            ViewBag.Filtro = filtro;
            ViewBag.FechaInicio = fInicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fFin.ToString("yyyy-MM-dd");

            return View(citas ?? new List<CitaViewModel>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CitaViewModel citaMedica)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    citaMedica.FechaCreacion = citaMedica.FechaCita;
                    citaMedica.UsuarioCreacion = usuarioLogueado;

                    var respuesta = await _apiProxy.SendRequestAsync<object>("CitaMedica", "InsertarCitaMedica", HttpMethod.Post, citaMedica);

                    if (respuesta != null)
                    {
                        TempData["MensajeExito"] = "La cita médica ha sido programada exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al conectar con la API: " + ex.Message);
                }
            }

            await CargarCatalogosCita();
            return View(citaMedica);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, new { idCita = id });
            var cita = citas?.FirstOrDefault(e => e.IdCita == id);

            if (cita == null)
            {
                TempData["Error"] = "No se encontró la cita solicitada.";
                return RedirectToAction("Index");
            }

            await CargarCatalogosCita();
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
                    var usuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    citaMedica.usuarioModificacion = usuarioLogueado;
                    citaMedica.FechaModificacion = DateTime.Now;

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

            await CargarCatalogosCita();
            return View(citaMedica);
        }

        public async Task<IActionResult> Programar()
        {
            await CargarCatalogosCita();
            return View(new CitaViewModel { FechaCita = DateTime.Now });
        }

        // MÉTODO AUXILIAR CORREGIDO PARA AMBAS VISTAS (CREAR Y EDITAR)
        private async Task CargarCatalogosCita()
        {
            var pacientes = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { });
            var medicos = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { });

            // Sincronizamos los nombres: Usamos "NombreConCedula" y "NombreConEspecialidad" 
            // para que coincidan con los SelectLists de las vistas.
            ViewBag.Pacientes = pacientes?.Select(p => new {
                p.IdPaciente,
                NombreConCedula = $"{p.Nombre} {p.Apellido} - {p.Identificacion}",
                NombreCompleto = $"{p.Nombre} {p.Apellido} - {p.Identificacion}" // Alias por si acaso
            }).ToList();

            ViewBag.Medicos = medicos?.Select(m => new {
                m.IdMedico,
                NombreConEspecialidad = $"{m.Nombre} {m.Apellido} {(string.IsNullOrEmpty(m.NumeroRegistroMINSA) ? "" : "- " + m.NumeroRegistroMINSA)}"
            }).ToList();
        }

        public async Task<IActionResult> Atender(int id)
        {
            if (id == 0) return RedirectToAction("Index");

            var filtro = new { idCita = id };
            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, filtro);
            var cita = citas?.FirstOrDefault(e => e.IdCita == id);

            if (cita == null) return RedirectToAction("Index");

            var consulta = new ConsultaViewModel
            {
                IdCita = cita.IdCita,
                NombrePaciente = cita.NombrePaciente,
                NombreMedico = cita.NombreMedico,
                MotivoCita = cita.Motivo,
                FechaConsulta = DateTime.Now,
            };

            return View(consulta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConsulta(ConsultaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                    var consultaApi = new
                    {
                        idCita = modelo.IdCita,
                        fechaConsulta = DateTime.Now,
                        diagnostico = modelo.Diagnostico,
                        tratamiento = modelo.Tratamiento,
                        notas = modelo.Notas,
                        fechaCreacion = DateTime.Now,
                        usuarioCreacion = usuarioLogueado,
                    };

                    var respuesta = await _apiProxy.SendRequestAsync<RespuestaConsulta>("Consulta", "InsertarConsulta", HttpMethod.Post, consultaApi);

                    if (respuesta != null)
                    {
                        TempData["ConsultaGuardadaId"] = respuesta.idConsulta;
                        return RedirectToAction(nameof(Index));

                        // return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al procesar la consulta: " + ex.Message);
                }
            }
            return View("Atender", modelo);
        }


        /// <summary>
        /// Recetas Medicas
        /// </summary>
        /// <param name="idConsulta"></param>
        /// <returns></returns>
        public IActionResult CrearReceta(int idConsulta)
        {
            return View(new RecetaViewModel { IdConsulta = idConsulta });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarReceta(RecetaViewModel modelo)
        {
            try
            {
                var usuarioLogueado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;

                var recetaApi = new
                {
                    idConsulta = modelo.IdConsulta,
                    medicamento = modelo.Medicamento,
                    dosis = modelo.Dosis,
                    frecuencia = modelo.Frecuencia,
                    duracion = modelo.Duracion,
                    usuarioCreacion = usuarioLogueado,
                };

                var respuesta = await _apiProxy.SendRequestAsync<object>("RecetaMedica", "InsertarRecetaMedica", HttpMethod.Post, recetaApi);

                if (respuesta != null)
                {
                    TempData["RecetaGuardadaIdConsulta"] = modelo.IdConsulta;
                    //return RedirectToAction("CrearReceta", modelo.IdConsulta);
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