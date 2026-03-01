namespace ConsultorioMedicoVerde.Models
{
    public class HistorialMedicoViewModel
    {
        public int IdHistorial { get; set; }

        public int IdPaciente { get; set; }

        public string? Alergias { get; set; }

        public string? EnfermedadesPrevias { get; set; }

        public string? CirugiasPrevias { get; set; }

        public string? Observaciones { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public string? UsuarioCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

        public string? UsuarioModificacion { get; set; }
    }
}
