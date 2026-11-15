namespace Dominio
{
    public class HistoriaClinica
    {
        public int Id { get; set; }
        public int TurnoId { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamiento { get; set; }
        public string Observaciones { get; set; }
    }
}
