using ConsultorioVerde.Web.Controllers;
using ConsultorioVerde.Web.Models;

namespace ConsultorioMedicoVerde.Models
{
    public class HomeViewModel
    {
        public int TotalPacientes { get; set; }
        public int TotalCitasHoy { get; set; } // Nueva propiedad
        public List<CitaViewModel> ProximasCitas { get; set; }


    }
}
