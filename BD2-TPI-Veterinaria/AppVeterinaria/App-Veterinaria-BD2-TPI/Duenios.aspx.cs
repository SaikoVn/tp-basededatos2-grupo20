using static Servicio.AccesoDatos;
using Servicio;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace VeterinariaWeb
{
    public partial class PaginaDuenios : PaginaClinica
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
                Texto(FiltroDni, false), Texto(FiltroNombre, false), Texto(FiltroApellido, false)));
        }

        protected void Crear_Click(object sender, EventArgs e)
        {
            Intentar(() =>
            {
                if (ViewState["DuenioEditando"] == null)
                {
                    int id = InsertarDuenio(Texto(Dni), Texto(Nombre), Texto(Apellido),
                        Texto(Telefono, false), Texto(Correo, false), Texto(Direccion, false));
                    Avisar("Dueño registrado. ID: " + id + ".", false);
                }
                else
                {
                    int id = (int)ViewState["DuenioEditando"];
                    Editar(id, Texto(Dni), Texto(Nombre), Texto(Apellido),
                        Texto(Telefono, false), Texto(Correo, false), Texto(Direccion, false));
                    Avisar("Dueño actualizado. ID: " + id + ".", false);
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
                if (datos.Rows.Count == 0) throw new ArgumentException("El dueño ya no existe.");
                DataRow fila = datos.Rows[0];
                Dominio.Duenio duenio = new Dominio.Duenio
                {
                    Id = id,
                    Dni = Convert.ToString(fila["DNI"]),
                    Nombre = Convert.ToString(fila["Nombre"]),
                    Apellido = Convert.ToString(fila["Apellido"]),
                    Telefono = Convert.ToString(fila["Telefono"]),
                    Email = Convert.ToString(fila["Correo"]),
                    Direccion = Convert.ToString(fila["Direccion"])
                };
                ViewState["DuenioEditando"] = id;
                Dni.Text = duenio.Dni;
                Nombre.Text = duenio.Nombre;
                Apellido.Text = duenio.Apellido;
                Telefono.Text = duenio.Telefono;
                Correo.Text = duenio.Email;
                Direccion.Text = duenio.Direccion;
                TituloFormulario.Text = "Editar dueño";
                Crear.Text = "Guardar cambios";
                CancelarEdicion.Visible = true;
            });
        }

        protected void CancelarEdicion_Click(object sender, EventArgs e)
        {
            LimpiarEdicion();
        }

        private void LimpiarEdicion()
        {
            ViewState.Remove("DuenioEditando");
            Dni.Text = Nombre.Text = Apellido.Text = Telefono.Text = Correo.Text = Direccion.Text = "";
            TituloFormulario.Text = "Registrar dueño";
            Crear.Text = "Guardar dueño";
            CancelarEdicion.Visible = false;
        }

        protected void Filtrar_Click(object sender, EventArgs e)
        {
            Intentar(Cargar);
        }
        private DataTable Listar(string dni, string nombre, string apellido)
        {
            dni = dni ?? "";
            nombre = nombre ?? "";
            apellido = apellido ?? "";
            return db.Consultar(@"SELECT id_duenio ID, dni DNI, nombre Nombre, apellido Apellido,
                    telefono Teléfono, email Correo, direccion Dirección
                    FROM dbo.Duenios
                    WHERE (@dni = '' OR dni = @dni)
                      AND (@nombre = '' OR nombre LIKE '%' + @nombre + '%')
                      AND (@apellido = '' OR apellido LIKE '%' + @apellido + '%')
                    ORDER BY apellido, nombre, id_duenio",
                Param("@dni", dni), Param("@nombre", nombre), Param("@apellido", apellido));
        }

        private int InsertarDuenio(string dni, string nombre, string apellido,
            string telefono, string email, string direccion)
        {
            var duenio = new Dominio.Duenio { Dni = dni, Nombre = nombre, Apellido = apellido, Telefono = telefono, Email = email, Direccion = direccion };

            DataTable resultado = db.Consultar(@"INSERT dbo.Duenios(dni, nombre, apellido, telefono, email, direccion)
                    OUTPUT INSERTED.id_duenio
                    VALUES(@dni, @nombre, @apellido, @telefono, @email, @direccion)",
                Param("@dni", duenio.Dni), Param("@nombre", duenio.Nombre), Param("@apellido", duenio.Apellido),
                Param("@telefono", duenio.Telefono), Param("@email", duenio.Email), Param("@direccion", duenio.Direccion));
            return (int)resultado.Rows[0][0];
        }

        private DataTable Obtener(int id)
        {
            return db.Consultar(@"SELECT id_duenio ID, dni DNI, nombre Nombre, apellido Apellido,
                    telefono Telefono, email Correo, direccion Direccion
                    FROM dbo.Duenios WHERE id_duenio = @id", Param("@id", id));
        }

        private void Editar(int id, string dni, string nombre, string apellido,
            string telefono, string email, string direccion)
        {
            var duenio = new Dominio.Duenio { Id = id, Dni = dni, Nombre = nombre, Apellido = apellido, Telefono = telefono, Email = email, Direccion = direccion };

            db.Ejecutar(@"UPDATE dbo.Duenios SET dni = @dni, nombre = @nombre,
                    apellido = @apellido, telefono = @telefono, email = @email, direccion = @direccion
                    WHERE id_duenio = @id;
                    IF @@ROWCOUNT = 0 THROW 50301, 'El dueño ya no existe.', 1;",
                Param("@id", duenio.Id), Param("@dni", duenio.Dni), Param("@nombre", duenio.Nombre),
                Param("@apellido", duenio.Apellido), Param("@telefono", duenio.Telefono),
                Param("@email", duenio.Email), Param("@direccion", duenio.Direccion));
        }

    }
}
