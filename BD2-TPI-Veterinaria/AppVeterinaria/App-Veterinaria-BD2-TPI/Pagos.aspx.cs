using static Servicio.AccesoDatos;
using Servicio;
using System.Data;
using System;

namespace VeterinariaWeb
{
    public partial class PaginaPagos : PaginaClinica
    {
        // Operaciones SQL de esta página.
        private readonly AccesoDatos db = new AccesoDatos();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            Intentar(Cargar);
        }

        private void Cargar()
        {
            DateTime? desde = FechaOpcional(DesdeLista);
            DateTime? hasta = FechaOpcional(HastaLista);
            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ArgumentException("La fecha desde debe ser anterior o igual a la fecha hasta.");
            Vincular(Tabla, Listar(IdOpcional(FiltroId), IdOpcional(FiltroTurno),
                Texto(FiltroComprobante, false), FiltroMetodo.SelectedValue, desde, hasta));
        }

        protected void Filtrar_Click(object sender, EventArgs e)
        {
            Intentar(Cargar);
        }
        private DataTable Listar(int id, int turno, string comprobante, string metodo,
            DateTime? desde, DateTime? hasta)
        {
            comprobante = comprobante ?? "";
            metodo = metodo ?? "";
            DateTime? hastaExclusivo = hasta.HasValue ? hasta.Value.Date.AddDays(1) : (DateTime?)null;
            return db.Consultar(@"SELECT pg.id_pago ID, pg.id_turno Turno, pg.nro_comprobante Comprobante,
                    pg.monto Monto, pg.metodo_pago Método, pg.fecha_pago Fecha,
                    m.nombre Mascota, CONCAT(p.apellido, ', ', p.nombre) Profesional
                    FROM dbo.Pagos pg
                    INNER JOIN dbo.Turnos t ON t.id_turno = pg.id_turno
                    INNER JOIN dbo.Mascotas m ON m.id_mascota = t.id_mascota
                    INNER JOIN dbo.Profesionales p ON p.id_profesional = t.id_profesional
                    WHERE (@id = 0 OR pg.id_pago = @id)
                      AND (@turno = 0 OR pg.id_turno = @turno)
                      AND (@comprobante = '' OR pg.nro_comprobante = @comprobante)
                      AND (@metodo = '' OR pg.metodo_pago = @metodo)
                      AND (@desde IS NULL OR pg.fecha_pago >= @desde)
                      AND (@hasta IS NULL OR pg.fecha_pago < @hasta)
                    ORDER BY pg.fecha_pago DESC, pg.id_pago DESC",
                Param("@id", id), Param("@turno", turno), Param("@comprobante", comprobante),
                Param("@metodo", metodo), Param("@desde", desde), Param("@hasta", hastaExclusivo));
        }

    }
}
