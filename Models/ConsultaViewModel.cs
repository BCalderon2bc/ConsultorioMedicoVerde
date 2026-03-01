namespace ConsultorioMedicoVerde.Models
{
    public class ConsultaViewModel
    {
        public int IdConsulta { get; set; }
        public int IdCita { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamiento { get; set; }
        public string? Notas { get; set; }
        public DateTime FechaConsulta { get; set; }

        // Propiedades adicionales para mostrar en la vista
        public string? NombrePaciente { get; set; }
        public string? NombreMedico { get; set; }
        public string? MotivoCita { get; set; }
    }
}
