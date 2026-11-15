using static Servicio.AccesoDatos;
using Servicio;
using System.Data;
using System;
using System.Web.UI;

namespace VeterinariaWeb
{
    public partial class PaginaAuditoria : PaginaClinica
    {
        // Operaciones SQL de esta página.
        private readonly AccesoDatos db = new AccesoDatos();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) Intentar(Cargar);
        }

        private void Cargar()
        {
            DateTime? desde = FechaOpcional(DesdeLista);
            DateTime? hasta = FechaOpcional(HastaLista);
            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ArgumentException("La fecha desde debe ser anterior o igual a la fecha hasta.");
            Vincular(Tabla, Listar(IdOpcional(FiltroId), IdOpcional(FiltroTurno),
                EventoLista.SelectedValue, desde, hasta));
        }

        protected void Filtrar_Click(object sender, EventArgs e)
        {
            Intentar(Cargar);
        }
        private DataTable Listar(int id, int turno, string evento, DateTime? desde, DateTime? hasta)
        {
            evento = evento ?? "";
            DateTime? hastaExclusivo = hasta.HasValue ? hasta.Value.Date.AddDays(1) : (DateTime?)null;
            return db.Consultar(@"SELECT id_auditoria ID, id_turno Turno,
                    tipo_evento Evento, fecha_evento Fecha, motivo_evento Motivo
                    FROM dbo.AuditoriaTurnos
                    WHERE (@id = 0 OR id_auditoria = @id)
                      AND (@turno = 0 OR id_turno = @turno)
                      AND (@evento = '' OR tipo_evento = @evento)
                      AND (@desde IS NULL OR fecha_evento >= @desde)
                      AND (@hasta IS NULL OR fecha_evento < @hasta)
                    ORDER BY fecha_evento DESC, id_auditoria DESC",
                Param("@id", id), Param("@turno", turno), Param("@evento", evento),
                Param("@desde", desde), Param("@hasta", hastaExclusivo));
        }

    }
}