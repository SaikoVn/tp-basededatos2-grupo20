namespace Dominio
{
    public class Profesional
    {
        public int Id { get; set; }
        public int EspecialidadId { get; set; }
        public string Matricula { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
    }
}
