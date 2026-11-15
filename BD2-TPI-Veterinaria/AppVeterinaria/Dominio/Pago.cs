namespace Dominio
{
    public class Pago
    {
        public int Id { get; set; }
        public int TurnoId { get; set; }
        public string NroComprobante { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; }
    }
}
