using static Servicio.AccesoDatos;
using Servicio;
using System.Data;
using System;
using System.Web.UI;

namespace VeterinariaWeb
{
    public partial class PaginaReporte : PaginaClinica
    {
        // Operaciones SQL de esta página.
        private readonly AccesoDatos db = new AccesoDatos();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) Intentar(() => Opciones(Profesional, OpcionesProfesionales()));
        }

        protected void Consultar_Click(object sender, EventArgs e)
        {
            Intentar(() =>
            {
                DateTime desde = Fecha(Desde).Date;
                DateTime hasta = Fecha(Hasta).Date;
                if (desde > hasta)
                    throw new ArgumentException("La fecha desde debe ser anterior o igual a la fecha hasta.");

                Vincular(Tabla, ConsultarReporte(Id(Profesional), desde, hasta));
                Avisar("Reporte consultado.", false);
            });
        }
        private DataTable OpcionesProfesionales()
        {
            return db.Consultar(@"SELECT id_profesional,
                    CONCAT(apellido, ', ', nombre, ' · ', matricula) Profesional
                    FROM dbo.Profesionales ORDER BY apellido, nombre");
        }

        private DataTable ConsultarReporte(int profesional, DateTime desde, DateTime hasta)
        {
            return db.Consultar(@"EXEC dbo.sp_ReporteTurnosProfesional
                    @id_profesional = @profesional, @desde = @desde,
                    @hasta_exclusivo = @hasta",
                Param("@profesional", profesional), Param("@desde", desde.Date),
                Param("@hasta", hasta.Date.AddDays(1)));
        }

    }
}