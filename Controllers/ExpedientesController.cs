
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
                    return NotFound(new { message = "Paciente no encontrado" });

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el expediente: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GenerarReporte(int idPaciente)
        {
            if (idPaciente <= 0)
                return BadRequest("IdPaciente inválido");

            // Aquí puedes generar el PDF usando tu servicio backend o algún generador local
            // Por simplicidad, redirigimos al API que genera el PDF
            var urlReporte = Url.Action("GenerarReportePDF", "ApiExpediente", new { idPaciente }, Request.Scheme);
            return Redirect(urlReporte);
        }

        public async Task<IActionResult> ExportarPdf(string identificacion)
        {

            //const filtroRequest = { Filtro: filtro };

          //await _apiProxy.SendRequestAsync<ExpedienteViewModel>(
          //       "Expediente",
          //       "ObtenerExpediente",
          //       HttpMethod.Post,
          //       filtroRequest
          //   );

            //var modelo = await ObtenerExpedienteCompleto(identificacion);

            //if (modelo == null) return NotFound();

            // 2. Retornar la vista como PDF
            return new ViewAsPdf("VistaReportePdf", "")
            {
                FileName = $"Expediente_{identificacion}.pdf",
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    //PageSize = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--page-offset 0 --footer-center [page]/[toPage] --footer-font-size 9"
                };
            }   

}
}