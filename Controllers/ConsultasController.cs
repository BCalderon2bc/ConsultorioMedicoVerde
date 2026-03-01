using ConsultorioMedicoVerde.Models;
using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConsultorioMedicoVerde.Controllers
{
    public class ConsultasController : Controller
    {
        private readonly ApiServiceProxy _apiProxy;

        public ConsultasController(ApiServiceProxy apiService)
        {
            _apiProxy = apiService;
        }

        // GET: Consultas/Atender/5
        public async Task<IActionResult> Atender(int id)
        {
            // 1. Buscamos la cita para traer el contexto (Paciente, Médico, Motivo)
            var filtro = new { idCita = id, activo = true };
            var citas = await _apiProxy.SendRequestAsync<List<CitaViewModel>>("CitaMedica", "ListarCitaMedica", HttpMethod.Post, filtro);
            var cita = citas?.FirstOrDefault();

            if (cita == null) return NotFound();

            // 2. Necesitamos los nombres (Si el API no los trae, los buscamos)
            var paciente = await _apiProxy.SendRequestAsync<List<PacienteViewModel>>("Paciente", "ListarPaciente", HttpMethod.Post, new { idPaciente = cita.IdPaciente });
            var medico = await _apiProxy.SendRequestAsync<List<MedicoViewModel>>("Medico", "ListarMedico", HttpMethod.Post, new { idMedico = cita.IdMedico });

            // 3. Preparamos el modelo de la consulta
            var modelo = new ConsultaViewModel
            {
                IdCita = cita.IdCita,
                NombrePaciente = paciente?.FirstOrDefault()?.NombreCompleto ?? "Desconocido",
                NombreMedico = medico?.FirstOrDefault()?.NombreCompleto ?? "Desconocido",
                MotivoCita = cita.Motivo,
                FechaConsulta = DateTime.Now
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(ConsultaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Datos para la tabla [Consulta]
                    var nuevaConsulta = new
                    {
                        idCita = modelo.IdCita,
                        diagnostico = modelo.Diagnostico,
                        tratamiento = modelo.Tratamiento,
                        notas = modelo.Notas,
                        fechaConsulta = DateTime.Now,
                        usuarioCreacion = User.Identity?.Name ?? "Medico_User",
                        fechaCreacion = DateTime.Now,
                        activo = true
                    };

                    // 2. Llamada al API de Consultas
                    var resultado = await _apiProxy.SendRequestAsync<object>("Consulta", "InsertarConsulta", HttpMethod.Post, nuevaConsulta);

                    if (resultado != null)
                    {
                        // 3. CAMBIO DE ESTADO: Actualizar la cita a "Completada"
                        // Aquí deberías llamar a un método que haga el PUT a CitaMedica/Actualizar

                        TempData["MensajeExito"] = "La consulta ha sido finalizada correctamente.";
                        return RedirectToAction("Index", "Citas");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar la consulta: " + ex.Message);
                }
            }
            return View("Atender", modelo);
        }
    }
}