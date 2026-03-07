
using ConsultorioMedicoVerde.Models;
using ConsultorioVerde.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;

namespace ConsultorioVerde.Web.Controllers
{
    public class ExpedientesController : Controller
    {

        private readonly ApiServiceProxy _apiProxy;
        public ExpedientesController(ApiServiceProxy apiService)
        {
            _apiProxy = apiService;
        }
        public IActionResult Index()
        {    
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ObtenerExpediente([FromBody] FiltroRequest filtroRequest)
        {
            if (string.IsNullOrWhiteSpace(filtroRequest.Filtro))
                return BadRequest(new { message = "El filtro es obligatorio" });

            try
            {
                // Aquí mandamos un objeto, no un string
                var resultado = await _apiProxy.SendRequestAsync<ExpedienteViewModel>(
                    "Expediente",
                    "ObtenerExpediente",
                    HttpMethod.Post,
                    filtroRequest
                );

                if (resultado == null || resultado.Paciente == null)
                    return Ok(new { paciente = (object)null });

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el expediente: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerarReporte(int idPaciente)
        {
            try
            {
                var response = await _apiProxy.GetByteArrayAsync(
                    "Expediente",
                    $"GenerarReporte?idPaciente={idPaciente}"
                );

                if (response == null)
                    return BadRequest("No se pudo generar el reporte.");

                return File(response, "application/pdf", $"Expediente_{idPaciente}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al generar reporte: " + ex.Message);
            }
        }

    }
}