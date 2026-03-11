namespace ConsultorioMedicoVerde.Models
{
    public class ResponseGeneric<T>
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
        public T Data { get; set; }
    }
}
