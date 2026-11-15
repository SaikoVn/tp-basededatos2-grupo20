using static Servicio.AccesoDatos;
using Servicio;
using System.Data;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace VeterinariaWeb
{
    public partial class PaginaProfesionales : PaginaClinica
    {
        // Operaciones SQL de esta página.
        private readonly AccesoDatos db = new AccesoDatos();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            Intentar(() =>
            {
                Opciones(Especialidad, Especialidades());
                Cargar();
            });
        }

        private void Cargar()
        {
            Vincular(Tabla, Listar(Texto(FiltroMatricula, false),
                Texto(FiltroNombre, false), Texto(FiltroApellido, false),
                Texto(FiltroEspecialidad, false)));
        }

        protected void Crear_Click(object sender, EventArgs e)
        {
            Intentar(() =>
            {
                if (ViewState["ProfesionalEditando"] == null)
                {
                    int id = InsertarProfesional(Id(Especialidad), Texto(Matricula), Texto(Nombre),
                        Texto(Apellido), Texto(Telefono, false), Texto(Correo, false));
                    Avisar("Profesional registrado. ID: " + id + ".", false);
                }
                else
                {
                    int id = (int)ViewState["ProfesionalEditando"];
                    Editar(id, Id(Especialidad), Texto(Matricula),
                        Texto(Nombre), Texto(Apellido), Texto(Telefono, false), Texto(Correo, false));
                    Avisar("Profesional actualizado. ID: " + id + ".", false);
                }
                LimpiarEdicion();
                Cargar();
            });
        }

        protected void Filtrar_Click(object sender, EventArgs e)
        {
            Intentar(Cargar);
        }

        protected void Tabla_Comando(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            if (e.CommandName == "Editar")
            {
                Intentar(() =>
                {
                    var datos = Obtener(id);
                    if (datos.Rows.Count == 0) throw new ArgumentException("El profesional ya no existe.");
                    var fila = datos.Rows[0];
                    Dominio.Profesional profesional = new Dominio.Profesional
                    {
                        Id = id,
                        EspecialidadId = Convert.ToInt32(fila["EspecialidadId"]),
                        Matricula = Convert.ToString(fila["Matricula"]),
                        Nombre = Convert.ToString(fila["Nombre"]),
                        Apellido = Convert.ToString(fila["Apellido"]),
                        Telefono = Convert.ToString(fila["Telefono"]),
                        Email = Convert.ToString(fila["Correo"])
                    };
                    ViewState["ProfesionalEditando"] = id;
                    Especialidad.SelectedValue = Convert.ToString(profesional.EspecialidadId);
                    Matricula.Text = profesional.Matricula;
                    Nombre.Text = profesional.Nombre;
                    Apellido.Text = profesional.Apellido;
                    Telefono.Text = profesional.Telefono;
                    Correo.Text = profesional.Email;
                    TituloFormulario.Text = "Editar profesional";
                    Crear.Text = "Guardar cambios";
                    CancelarEdicion.Visible = true;
                });
                return;
            }
            bool activar = e.CommandName == "Activar";
            Intentar(() =>
            {
                CambiarEstado(id, activar);
                Avisar((activar ? "Profesional activado." : "Profesional desactivado.") + " ID: " + id + ".", false);
                Cargar();
            });
        }

        protected void CancelarEdicion_Click(object sender, EventArgs e)
        {
            LimpiarEdicion();
        }

        private void LimpiarEdicion()
        {
            ViewState.Remove("ProfesionalEditando");
            Especialidad.SelectedIndex = 0;
            Nombre.Text = Apellido.Text = Matricula.Text = Telefono.Text = Correo.Text = "";
            TituloFormulario.Text = "Registrar profesional";
            Crear.Text = "Guardar profesional";
            CancelarEdicion.Visible = false;
        }
        private DataTable Listar(string matricula, string nombre, string apellido, string especialidad)
        {
            matricula = matricula ?? "";
            nombre = nombre ?? "";
            apellido = apellido ?? "";
            especialidad = especialidad ?? "";
            return db.Consultar(@"SELECT p.id_profesional ID, p.matricula Matrícula,
                    CONCAT(p.apellido, ', ', p.nombre) Profesional, e.nombre Especialidad,
                    p.telefono Teléfono, p.email Correo, p.activo Estado
                    FROM dbo.Profesionales p
                    INNER JOIN dbo.Especialidades e ON e.id_especialidad = p.id_especialidad
                    WHERE (@matricula = '' OR p.matricula = @matricula)
                      AND (@nombre = '' OR p.nombre LIKE '%' + @nombre + '%')
                      AND (@apellido = '' OR p.apellido LIKE '%' + @apellido + '%')
                      AND (@especialidad = '' OR e.nombre LIKE '%' + @especialidad + '%')
                    ORDER BY p.apellido, p.nombre, p.id_profesional",
                Param("@matricula", matricula), Param("@nombre", nombre),
                Param("@apellido", apellido), Param("@especialidad", especialidad));
        }

        private DataTable Especialidades()
        {
            return db.Consultar(@"SELECT id_especialidad, nombre FROM dbo.Especialidades ORDER BY nombre");
        }

        private int InsertarProfesional(int especialidad, string matricula, string nombre, string apellido,
            string telefono, string correo)
        {
            var profesional = new Dominio.Profesional { EspecialidadId = especialidad, Matricula = matricula, Nombre = nombre, Apellido = apellido, Telefono = telefono, Email = correo, Activo = true };

            DataTable resultado = db.Consultar(@"INSERT dbo.Profesionales
                    (id_especialidad, matricula, nombre, apellido, telefono, email, activo)
                    OUTPUT INSERTED.id_profesional
                    VALUES(@especialidad, @matricula, @nombre, @apellido, @telefono, @correo, 1)",
                Param("@especialidad", profesional.EspecialidadId), Param("@matricula", profesional.Matricula),
                Param("@nombre", profesional.Nombre), Param("@apellido", profesional.Apellido),
                Param("@telefono", profesional.Telefono), Param("@correo", profesional.Email));
            return (int)resultado.Rows[0][0];
        }

        private DataTable Obtener(int id)
        {
            return db.Consultar(@"SELECT id_profesional ID, id_especialidad EspecialidadId,
                    matricula Matricula, nombre Nombre, apellido Apellido,
                    telefono Telefono, email Correo
                    FROM dbo.Profesionales WHERE id_profesional = @id", Param("@id", id));
        }

        private void Editar(int id, int especialidad, string matricula, string nombre, string apellido,
            string telefono, string correo)
        {
            var profesional = new Dominio.Profesional { Id = id, EspecialidadId = especialidad, Matricula = matricula, Nombre = nombre, Apellido = apellido, Telefono = telefono, Email = correo };

            db.Ejecutar(@"UPDATE dbo.Profesionales SET id_especialidad = @especialidad,
                    matricula = @matricula, nombre = @nombre, apellido = @apellido,
                    telefono = @telefono, email = @correo
                    WHERE id_profesional = @id;
                    IF @@ROWCOUNT = 0 THROW 50303, 'El profesional ya no existe.', 1;",
                Param("@id", profesional.Id), Param("@especialidad", profesional.EspecialidadId),
                Param("@matricula", profesional.Matricula), Param("@nombre", profesional.Nombre),
                Param("@apellido", profesional.Apellido), Param("@telefono", profesional.Telefono), Param("@correo", profesional.Email));
        }

        private void CambiarEstado(int id, bool activo)
        {
            db.Ejecutar(@"IF @activo = 0 AND EXISTS
                    (SELECT 1 FROM dbo.Turnos WHERE id_profesional = @id AND estado = 'pendiente')
                    THROW 50201, 'Resolvé los turnos pendientes antes de desactivar al profesional.', 1;
                    UPDATE dbo.Profesionales SET activo = @activo WHERE id_profesional = @id",
                Param("@id", id), Param("@activo", activo));
        }

    }
}
