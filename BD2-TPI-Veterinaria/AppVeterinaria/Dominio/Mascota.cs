using System;

namespace Dominio
{
    public class Mascota
    {
        public int Id { get; set; }
        public int DuenioId { get; set; }
        public string Nombre { get; set; }
        public string Especie { get; set; }
        public string Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Sexo { get; set; }
    }
}
