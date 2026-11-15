using static Servicio.AccesoDatos;
using Servicio;
using System.Data;
using System;
using System.Web.UI;

namespace VeterinariaWeb
{
    public partial class PaginaHistorial : PaginaClinica
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
            Vincular(Tabla, Listar(Texto(FiltroMascota, false),
                Texto(FiltroDuenio, false), IdOpcional(FiltroTurno), desde, hasta));
        }

        protected void Filtrar_Click(object sender, EventArgs e)
        {
            Intentar(Cargar);
        }
        private DataTable Listar(string mascota, string duenio, int turno,
            DateTime? desde, DateTime? hasta)
        {
            mascota = mascota ?? "";
            duenio = duenio ?? "";
            DateTime? hastaExclusivo = hasta.HasValue ? hasta.Value.Date.AddDays(1) : (DateTime?)null;
            return db.Consultar(@"SELECT id_historia Historia, mascota Mascota, especie Especie,
                    mascota_activa Estado, CONCAT(nombre_duenio, ' ', apellido_duenio) Dueño,
                    id_turno Turno, fecha_hora_inicio Inicio,
                    CONCAT(nombre_profesional, ' ', apellido_profesional) Profesional,
                    fecha_registro Registro, diagnostico Diagnóstico,
                    tratamiento Tratamiento, observaciones Observaciones
                    FROM dbo.vw_HistorialClinicoPorMascota
                    WHERE (@mascota = '' OR mascota LIKE '%' + @mascota + '%')
                      AND (@duenio = '' OR nombre_duenio LIKE '%' + @duenio + '%'
                           OR apellido_duenio LIKE '%' + @duenio + '%')
                      AND (@turno = 0 OR id_turno = @turno)
                      AND (@desde IS NULL OR fecha_registro >= @desde)
                      AND (@hasta IS NULL OR fecha_registro < @hasta)
                    ORDER BY fecha_registro DESC, id_historia DESC",
                Param("@mascota", mascota), Param("@duenio", duenio), Param("@turno", turno),
                Param("@desde", desde), Param("@hasta", hastaExclusivo));
        }

    }
}