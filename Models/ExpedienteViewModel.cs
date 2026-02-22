namespace ConsultorioVerde.Web.Models
{
    public class ExpedienteViewModel
    {
        public int IdExpediente { get; set; }
        public int IdPaciente { get; set; }
        public string NombrePaciente { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string MotivoConsulta { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamiento { get; set; }
        public string Observaciones { get; set; }
    }
}