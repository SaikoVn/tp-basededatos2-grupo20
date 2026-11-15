using static Servicio.AccesoDatos;
using Servicio;
using System.Data;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace VeterinariaWeb
{
    public partial class PaginaMascotas : PaginaClinica
    {
        // Operaciones SQL de esta página.
        private readonly AccesoDatos db = new AccesoDatos();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            Intentar(() =>
            {
                Opciones(Duenio, OpcionesDuenios());
                Cargar();
            });
        }

        private void Cargar()
        {
            Vincular(Tabla, Listar(Texto(FiltroNombre, false),
                Texto(FiltroEspecie, false), Texto(FiltroRaza, false),
                Texto(FiltroDuenio, false), EstadoLista.SelectedValue));
        }

        protected void Crear_Click(object sender, EventArgs e)
        {
            Intentar(() =>
            {
                if (ViewState["MascotaEditando"] == null)
                {
                    int id = InsertarMascota(Id(Duenio), Texto(Nombre), Texto(Especie),
                        Texto(Raza, false), FechaOpcional(Nacimiento),
                        string.IsNullOrEmpty(Sexo.SelectedValue) ? null : Sexo.SelectedValue);
                    Avisar("Mascota registrada. ID: " + id + ".", false);
                }
                else
                {
                    int id = (int)ViewState["MascotaEditando"];
                    Editar(id, Id(Duenio), Texto(Nombre), Texto(Especie),
                        Texto(Raza, false), FechaOpcional(Nacimiento),
                        string.IsNullOrEmpty(Sexo.SelectedValue) ? null : Sexo.SelectedValue);
                    Avisar("Mascota actualizada. ID: " + id + ".", false);
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
                    if (datos.Rows.Count == 0) throw new ArgumentException("La mascota ya no existe.");
                    var fila = datos.Rows[0];
                    Dominio.Mascota mascota = new Dominio.Mascota
                    {
                        Id = id,
                        DuenioId = Convert.ToInt32(fila["DuenioId"]),
                        Nombre = Convert.ToString(fila["Nombre"]),
                        Especie = Convert.ToString(fila["Especie"]),
                        Raza = Convert.ToString(fila["Raza"]),
                        FechaNacimiento = fila["Nacimiento"] == DBNull.Value
                            ? (DateTime?)null : Convert.ToDateTime(fila["Nacimiento"]),
                        Sexo = fila["Sexo"] == DBNull.Value ? null : Convert.ToString(fila["Sexo"])
                    };
                    ViewState["MascotaEditando"] = id;
                    Duenio.SelectedValue = Convert.ToString(mascota.DuenioId);
                    Nombre.Text = mascota.Nombre;
                    Especie.Text = mascota.Especie;
                    Raza.Text = mascota.Raza;
                    Nacimiento.Text = mascota.FechaNacimiento.HasValue
                        ? mascota.FechaNacimiento.Value.ToString("yyyy-MM-dd") : "";
                    Sexo.SelectedValue = mascota.Sexo ?? "";
                    TituloFormulario.Text = "Editar mascota";
                    Crear.Text = "Guardar cambios";
                    CancelarEdicion.Visible = true;
                });
                return;
            }
            bool activar = e.CommandName == "Reactivar";
            Intentar(() =>
            {
                CambiarEstado(id, activar);
                Avisar((activar ? "Mascota reactivada." : "Mascota dada de baja.") + " ID: " + id + ".", false);
                Cargar();
            });
        }

        protected void CancelarEdicion_Click(object sender, EventArgs e)
        {
            LimpiarEdicion();
        }

        private void LimpiarEdicion()
        {
            ViewState.Remove("MascotaEditando");
            Duenio.SelectedIndex = 0;
            Nombre.Text = Especie.Text = Raza.Text = Nacimiento.Text = "";
            Sexo.SelectedIndex = 0;
            TituloFormulario.Text = "Registrar mascota";
            Crear.Text = "Guardar mascota";
            CancelarEdicion.Visible = false;
        }
        private DataTable Listar(string nombre, string especie, string raza, string duenio, string estado)
        {
            nombre = nombre ?? "";
            especie = especie ?? "";
            raza = raza ?? "";
            duenio = duenio ?? "";
            estado = estado ?? "";
            return db.Consultar(@"SELECT m.id_mascota ID, m.nombre Mascota, m.especie Especie,
                    m.raza Raza, m.sexo Sexo, m.fecha_nacimiento Nacimiento,
                    CONCAT(d.apellido, ', ', d.nombre) Dueño, m.activo Estado
                    FROM dbo.Mascotas m
                    INNER JOIN dbo.Duenios d ON d.id_duenio = m.id_duenio
                    WHERE (@nombre = '' OR m.nombre LIKE '%' + @nombre + '%')
                      AND (@especie = '' OR m.especie LIKE '%' + @especie + '%')
                      AND (@raza = '' OR m.raza LIKE '%' + @raza + '%')
                      AND (@duenio = '' OR d.nombre LIKE '%' + @duenio + '%'
                           OR d.apellido LIKE '%' + @duenio + '%')
                      AND (@estado = '' OR CONVERT(varchar(1), m.activo) = @estado)
                    ORDER BY m.nombre, m.id_mascota",
                Param("@nombre", nombre), Param("@especie", especie), Param("@raza", raza),
                Param("@duenio", duenio), Param("@estado", estado));
        }

        private DataTable OpcionesDuenios()
        {
            return db.Consultar(@"SELECT id_duenio, CONCAT(apellido, ', ', nombre, ' · ', dni) Dueño
                    FROM dbo.Duenios ORDER BY apellido, nombre");
        }

        private int InsertarMascota(int duenio, string nombre, string especie, string raza,
            DateTime? nacimiento, string sexo)
        {
            var mascota = new Dominio.Mascota { DuenioId = duenio, Nombre = nombre, Especie = especie, Raza = raza, FechaNacimiento = nacimiento, Sexo = sexo };

            DataTable resultado = db.Consultar(@"INSERT dbo.Mascotas(id_duenio, nombre, especie, raza, fecha_nacimiento, sexo)
                    OUTPUT INSERTED.id_mascota
                    VALUES(@duenio, @nombre, @especie, @raza, @nacimiento, @sexo)",
                Param("@duenio", mascota.DuenioId), Param("@nombre", mascota.Nombre), Param("@especie", mascota.Especie),
                Param("@raza", mascota.Raza), Param("@nacimiento", mascota.FechaNacimiento), Param("@sexo", mascota.Sexo));
            return (int)resultado.Rows[0][0];
        }

        private DataTable Obtener(int id)
        {
            return db.Consultar(@"SELECT id_mascota ID, id_duenio DuenioId, nombre Nombre,
                    especie Especie, raza Raza, fecha_nacimiento Nacimiento, sexo Sexo
                    FROM dbo.Mascotas WHERE id_mascota = @id", Param("@id", id));
        }

        private void Editar(int id, int duenio, string nombre, string especie, string raza,
            DateTime? nacimiento, string sexo)
        {
            var mascota = new Dominio.Mascota { Id = id, DuenioId = duenio, Nombre = nombre, Especie = especie, Raza = raza, FechaNacimiento = nacimiento, Sexo = sexo };

            db.Ejecutar(@"UPDATE dbo.Mascotas SET id_duenio = @duenio, nombre = @nombre,
                    especie = @especie, raza = @raza, fecha_nacimiento = @nacimiento, sexo = @sexo
                    WHERE id_mascota = @id;
                    IF @@ROWCOUNT = 0 THROW 50302, 'La mascota ya no existe.', 1;",
                Param("@id", mascota.Id), Param("@duenio", mascota.DuenioId), Param("@nombre", mascota.Nombre),
                Param("@especie", mascota.Especie), Param("@raza", mascota.Raza),
                Param("@nacimiento", mascota.FechaNacimiento), Param("@sexo", mascota.Sexo));
        }

        private void CambiarEstado(int id, bool activo)
        {
            db.Ejecutar(@"IF @activo = 0 AND EXISTS
                    (SELECT 1 FROM dbo.Turnos WHERE id_mascota = @id AND estado = 'pendiente')
                    THROW 50103, 'Resolvé los turnos pendientes antes de dar de baja a la mascota.', 1;
                    UPDATE dbo.Mascotas SET activo = @activo WHERE id_mascota = @id",
                Param("@id", id), Param("@activo", activo));
        }

    }
}
