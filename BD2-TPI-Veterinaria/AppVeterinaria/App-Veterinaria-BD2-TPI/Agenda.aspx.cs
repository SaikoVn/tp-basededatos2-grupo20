using static Servicio.AccesoDatos;
using Servicio;
using System.Data;
using System;
using System.Web.UI;

namespace VeterinariaWeb
{
    public partial class PaginaAgenda : PaginaClinica
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
                Texto(FiltroProfesional, false), EstadoLista.SelectedValue, desde, hasta));
        }

        protected void Filtrar_Click(object sender, EventArgs e)
        {
            Intentar(Cargar);
        }
        private DataTable Listar(string mascota, string profesional, string estado,
            DateTime? desde, DateTime? hasta)
        {
            mascota = mascota ?? "";
            profesional = profesional ?? "";
            estado = estado ?? "";
            DateTime? hastaExclusivo = hasta.HasValue ? hasta.Value.Date.AddDays(1) : (DateTime?)null;
            return db.Consultar(@"SELECT id_turno Turno, fecha_hora_inicio Inicio,
                    fecha_hora_fin Fin, mascota Mascota,
                    CONCAT(nombre_duenio, ' ', apellido_duenio) Dueño,
                    CONCAT(nombre_profesional, ' ', apellido_profesional) Profesional,
                    especialidad Especialidad, estado Estado, motivo Motivo
                    FROM dbo.vw_AgendaTurnosActivos
                    WHERE (@mascota = '' OR mascota LIKE '%' + @mascota + '%')
                      AND (@profesional = '' OR nombre_profesional LIKE '%' + @profesional + '%'
                           OR apellido_profesional LIKE '%' + @profesional + '%')
                      AND (@estado = '' OR estado = @estado)
                      AND (@desde IS NULL OR fecha_hora_inicio >= @desde)
                      AND (@hasta IS NULL OR fecha_hora_inicio < @hasta)
                    ORDER BY fecha_hora_inicio, id_turno",
                Param("@mascota", mascota), Param("@profesional", profesional),
                Param("@estado", estado), Param("@desde", desde), Param("@hasta", hastaExclusivo));
        }

    }
}