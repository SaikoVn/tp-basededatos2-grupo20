using System;

namespace Dominio
{
    public class Turno
    {
        public int Id { get; set; }
        public int MascotaId { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public string Motivo { get; set; }
    }
}
