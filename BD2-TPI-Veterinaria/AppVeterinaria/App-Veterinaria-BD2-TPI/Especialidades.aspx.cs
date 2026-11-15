using static Servicio.AccesoDatos;
using Servicio;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace VeterinariaWeb
{
    public partial class PaginaEspecialidades : PaginaClinica
    {
        // Operaciones SQL de esta página.
        private readonly AccesoDatos db = new AccesoDatos();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) Intentar(Cargar);
        }

        private void Cargar()
        {
            Vincular(Tabla, Listar());
        }

        protected void Guardar_Click(object sender, EventArgs e)
        {
            Intentar(() =>
            {
                string nombre = Texto(Nombre);
                if (ViewState["EspecialidadEditando"] == null)
                {
                    int id = Crear(nombre);
                    Avisar("Especialidad agregada. ID: " + id + ".", false);
                }
                else
                {
                    int id = (int)ViewState["EspecialidadEditando"];
                    Editar(id, nombre);
                    Avisar("Especialidad actualizada. ID: " + id + ".", false);
                }
                LimpiarEdicion();
                Cargar();
            });
        }

        protected void Tabla_Comando(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (e.CommandName != "Editar" || !int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            Intentar(() =>
            {
                DataTable datos = Obtener(id);
                if (datos.Rows.Count == 0) throw new ArgumentException("La especialidad ya no existe.");
                Dominio.Especialidad especialidad = new Dominio.Especialidad
                {
                    Id = id,
                    Nombre = Convert.ToString(datos.Rows[0]["Nombre"])
                };
                ViewState["EspecialidadEditando"] = id;
                Nombre.Text = especialidad.Nombre;
                TituloFormulario.Text = "Editar especialidad";
                Guardar.Text = "Guardar cambios";
                Cancelar.Visible = true;
            });
        }

        protected void Cancelar_Click(object sender, EventArgs e)
        {
            LimpiarEdicion();
        }

        private void LimpiarEdicion()
        {
            ViewState.Remove("EspecialidadEditando");
            Nombre.Text = "";
            TituloFormulario.Text = "Agregar especialidad";
            Guardar.Text = "Guardar especialidad";
            Cancelar.Visible = false;
        }
        private DataTable Listar()
        {
            return db.Consultar(@"SELECT id_especialidad ID, nombre Nombre
                    FROM dbo.Especialidades ORDER BY nombre, id_especialidad");
        }

        private DataTable Obtener(int id)
        {
            return db.Consultar(@"SELECT id_especialidad ID, nombre Nombre
                    FROM dbo.Especialidades WHERE id_especialidad = @id",
                Param("@id", id));
        }

        private int Crear(string nombre)
        {
            var especialidad = new Dominio.Especialidad { Nombre = nombre };

            DataTable resultado = db.Consultar(@"INSERT dbo.Especialidades(nombre)
                    OUTPUT INSERTED.id_especialidad VALUES(@nombre)",
                Param("@nombre", especialidad.Nombre));
            return (int)resultado.Rows[0][0];
        }

        private void Editar(int id, string nombre)
        {
            var especialidad = new Dominio.Especialidad { Id = id, Nombre = nombre };

            db.Ejecutar(@"UPDATE dbo.Especialidades SET nombre = @nombre
                    WHERE id_especialidad = @id;
                    IF @@ROWCOUNT = 0 THROW 50202, 'La especialidad ya no existe.', 1;",
                Param("@id", especialidad.Id), Param("@nombre", especialidad.Nombre));
        }

    }
}
