using static Servicio.AccesoDatos;
using Servicio;
using System.Data;
using System;
using System.Web.UI;

namespace VeterinariaWeb
{
    public partial class PaginaFacturacion : PaginaClinica
    {
        // Operaciones SQL de esta página.
        private readonly AccesoDatos db = new AccesoDatos();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) Intentar(Cargar);
        }

        private void Cargar()
        {
            Vincular(Tabla, Listar(
                Texto(FiltroMatricula, false), Texto(FiltroProfesional, false)));
        }

        protected void Filtrar_Click(object sender, EventArgs e)
        {
            Intentar(Cargar);
        }
        private DataTable Listar(string matricula, string profesional)
        {
            matricula = matricula ?? "";
            profesional = profesional ?? "";
            return db.Consultar(@"SELECT id_profesional ID, matricula Matrícula,
                    CONCAT(apellido_profesional, ', ', nombre_profesional) Profesional,
                    cantidad_pagos Pagos, total_facturado Total
                    FROM dbo.vw_ResumenFacturacionPorProfesional
                    WHERE (@matricula = '' OR matricula = @matricula)
                      AND (@profesional = '' OR nombre_profesional LIKE '%' + @profesional + '%'
                           OR apellido_profesional LIKE '%' + @profesional + '%')
                    ORDER BY apellido_profesional, nombre_profesional, id_profesional",
                Param("@matricula", matricula), Param("@profesional", profesional));
        }

    }
}