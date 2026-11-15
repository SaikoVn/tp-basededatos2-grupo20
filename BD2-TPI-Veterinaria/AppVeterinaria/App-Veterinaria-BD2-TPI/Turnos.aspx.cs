using static Servicio.AccesoDatos;
using Servicio;
using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace VeterinariaWeb
{
    public partial class PaginaTurnos : PaginaClinica
    {
        // Operaciones SQL de esta página.
        private readonly AccesoDatos db = new AccesoDatos();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            Intentar(() =>
            {
                Opciones(Mascota, OpcionesMascotas());
                Opciones(Profesional, OpcionesProfesionales());
                Cargar();
            });
        }

        private void Cargar()
        {
            DateTime? desde = FechaOpcional(DesdeLista);
            DateTime? hasta = FechaOpcional(HastaLista);
            ValidarRango(desde, hasta);
            Vincular(Tabla, Listar(IdOpcional(FiltroId),
                Texto(FiltroMascota, false), Texto(FiltroProfesional, false),
                FiltroEstado.SelectedValue, desde, hasta));
        }

        protected void Crear_Click(object sender, EventArgs e)
        {
            Intentar(() =>
            {
                DateTime inicio = Fecha(Inicio);
                DateTime fin = Fecha(Fin);
                ValidarHorario(inicio, fin);
                int id = Crear(Id(Mascota), Id(Profesional), inicio, fin, Texto(Motivo, false));
                Avisar("Turno registrado. ID: " + id + ".", false);
                Inicio.Text = Fin.Text = Motivo.Text = "";
                Mascota.SelectedIndex = Profesional.SelectedIndex = 0;
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
            Intentar(() =>
            {
                if (e.CommandName == "Editar")
                {
                    DataTable datos = Obtener(id);
                    if (datos.Rows.Count == 0) throw new ArgumentException("El turno ya no existe.");
                    DataRow fila = datos.Rows[0];
                    Dominio.Turno turno = new Dominio.Turno
                    {
                        Id = id,
                        ProfesionalId = Convert.ToInt32(fila["ProfesionalId"]),
                        Inicio = Convert.ToDateTime(fila["Inicio"]),
                        Fin = Convert.ToDateTime(fila["Fin"]),
                        Motivo = Convert.ToString(fila["Motivo"])
                    };
                    ViewState["TurnoEditando"] = id;
                    Opciones(NuevoProfesional, OpcionesProfesionales(turno.ProfesionalId));
                    NuevoProfesional.SelectedValue = Convert.ToString(turno.ProfesionalId);
                    NuevoInicio.Text = turno.Inicio.ToString("yyyy-MM-ddTHH:mm");
                    NuevoFin.Text = turno.Fin.ToString("yyyy-MM-ddTHH:mm");
                    NuevoMotivo.Text = turno.Motivo;
                    Edicion.Visible = true;
                }
                else if (e.CommandName == "Atender")
                {
                    DataTable datos = Obtener(id);
                    if (datos.Rows.Count == 0) throw new ArgumentException("El turno ya no existe.");
                    ViewState["TurnoAtencion"] = id;
                    TurnoAtencion.Text = "#" + id;
                    Diagnostico.Text = Tratamiento.Text = Observaciones.Text = "";
                    ComprobanteAtencion.Text = MontoAtencion.Text = "";
                    MetodoAtencion.SelectedIndex = 0;
                    Atencion.Visible = true;
                }
                else if (e.CommandName == "Cancelar")
                {
                    Cancelar(id);
                    Avisar("Turno cancelado. ID: " + id + ".", false);
                }
                else if (e.CommandName == "Eliminar")
                {
                    Eliminar(id);
                    Avisar("Turno eliminado. ID: " + id + ".", false);
                }
                Cargar();
            });
        }

        protected void GuardarHorario_Click(object sender, EventArgs e)
        {
            Intentar(() =>
            {
                if (ViewState["TurnoEditando"] == null)
                    throw new ArgumentException("Volvé a seleccionar el turno.");
                DateTime inicio = Fecha(NuevoInicio);
                DateTime fin = Fecha(NuevoFin);
                ValidarHorario(inicio, fin);
                int id = (int)ViewState["TurnoEditando"];
                Editar(id, Id(NuevoProfesional),
                    inicio, fin, Texto(NuevoMotivo, false));
                Edicion.Visible = false;
                Avisar("Turno actualizado. ID: " + id + ".", false);
                Cargar();
            });
        }

        protected void Volver_Click(object sender, EventArgs e)
        {
            Edicion.Visible = false;
            ViewState.Remove("TurnoEditando");
        }

        protected void GuardarAtencion_Click(object sender, EventArgs e)
        {
            Intentar(() =>
            {
                if (ViewState["TurnoAtencion"] == null)
                    throw new ArgumentException("Volvé a seleccionar el turno.");
                decimal monto;
                if (!decimal.TryParse(MontoAtencion.Text, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out monto) || monto <= 0)
                    throw new ArgumentException("Ingresá un monto mayor a cero.");
                int id = (int)ViewState["TurnoAtencion"];
                DataTable resultado = Registrar(id,
                    Texto(Diagnostico), Texto(Tratamiento, false), Texto(Observaciones, false),
                    Texto(ComprobanteAtencion), monto, MetodoAtencion.SelectedValue);
                int historia = Convert.ToInt32(resultado.Rows[0]["Historia"]);
                int pago = Convert.ToInt32(resultado.Rows[0]["Pago"]);
                Atencion.Visible = false;
                ViewState.Remove("TurnoAtencion");
                Avisar("Atención y pago registrados. Turno ID: " + id + ", ficha ID: " + historia
                    + ", pago ID: " + pago + ". El turno quedó atendido.", false);
                Cargar();
            });
        }

        protected void CancelarAtencion_Click(object sender, EventArgs e)
        {
            Atencion.Visible = false;
            ViewState.Remove("TurnoAtencion");
        }

        private static void ValidarRango(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ArgumentException("La fecha desde debe ser anterior o igual a la fecha hasta.");
        }

        private static void ValidarHorario(DateTime inicio, DateTime fin)
        {
            if (inicio.Date != fin.Date)
                throw new ArgumentException("El inicio y el fin del turno deben ser el mismo día.");
            if (fin <= inicio)
                throw new ArgumentException("La hora de fin debe ser posterior a la hora de inicio.");
        }
        private DataTable Listar(int id, string mascota, string profesional, string estado,
            DateTime? desde, DateTime? hasta)
        {
            mascota = mascota ?? "";
            profesional = profesional ?? "";
            estado = estado ?? "";
            DateTime? hastaExclusivo = hasta.HasValue ? hasta.Value.Date.AddDays(1) : (DateTime?)null;

            return db.Consultar(@"SELECT t.id_turno ID, m.nombre Mascota,
                    CONCAT(p.apellido, ', ', p.nombre) Profesional,
                    t.fecha_hora_inicio Inicio, t.fecha_hora_fin Fin,
                    t.estado Estado, t.motivo Motivo,
                    CASE WHEN h.id_turno IS NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END PuedeAtender
                    FROM dbo.Turnos t
                    INNER JOIN dbo.Mascotas m ON m.id_mascota = t.id_mascota
                    INNER JOIN dbo.Profesionales p ON p.id_profesional = t.id_profesional
                    LEFT JOIN dbo.HistoriasClinicas h ON h.id_turno = t.id_turno
                    WHERE (@id = 0 OR t.id_turno = @id)
                      AND (@mascota = '' OR m.nombre LIKE '%' + @mascota + '%')
                      AND (@profesional = '' OR p.nombre LIKE '%' + @profesional + '%'
                           OR p.apellido LIKE '%' + @profesional + '%')
                      AND (@estado = '' OR t.estado = @estado)
                      AND (@desde IS NULL OR t.fecha_hora_inicio >= @desde)
                      AND (@hasta IS NULL OR t.fecha_hora_inicio < @hasta)
                    ORDER BY t.fecha_hora_inicio DESC, t.id_turno DESC",
                Param("@id", id), Param("@mascota", mascota), Param("@profesional", profesional),
                Param("@estado", estado), Param("@desde", desde), Param("@hasta", hastaExclusivo));
        }

        private DataTable OpcionesMascotas()
        {
            return db.Consultar(@"SELECT id_mascota,
                    CONCAT(nombre, ' · #', id_mascota) Mascota
                    FROM dbo.Mascotas WHERE activo = 1 ORDER BY nombre");
        }

        private DataTable OpcionesProfesionales(int incluir = 0)
        {
            return db.Consultar(@"SELECT id_profesional,
                    CONCAT(apellido, ', ', nombre) Profesional
                    FROM dbo.Profesionales
                    WHERE activo = 1 OR id_profesional = @incluir
                    ORDER BY apellido, nombre",
                Param("@incluir", incluir));
        }

        private DataTable Obtener(int id)
        {
            return db.Consultar(@"SELECT id_turno ID, id_profesional ProfesionalId,
                    fecha_hora_inicio Inicio, fecha_hora_fin Fin, motivo Motivo
                    FROM dbo.Turnos WHERE id_turno = @id",
                Param("@id", id));
        }

        private int Crear(int mascota, int profesional, DateTime inicio, DateTime fin, string motivo)
        {
            var turno = new Dominio.Turno { MascotaId = mascota, ProfesionalId = profesional, Inicio = inicio, Fin = fin, Motivo = motivo };

            DataTable resultado = db.Consultar(@"DECLARE @nuevo TABLE(id_turno INT);
                    INSERT dbo.Turnos
                    (id_mascota, id_profesional, fecha_hora_inicio, fecha_hora_fin, motivo)
                    OUTPUT INSERTED.id_turno INTO @nuevo
                    VALUES(@mascota, @profesional, @inicio, @fin, @motivo);
                    SELECT id_turno FROM @nuevo",
                Param("@mascota", turno.MascotaId), Param("@profesional", turno.ProfesionalId),
                Param("@inicio", turno.Inicio), Param("@fin", turno.Fin), Param("@motivo", turno.Motivo));
            return (int)resultado.Rows[0][0];
        }

        private void Editar(int id, int profesional, DateTime inicio, DateTime fin, string motivo)
        {
            var turno = new Dominio.Turno { Id = id, ProfesionalId = profesional, Inicio = inicio, Fin = fin, Motivo = motivo };

            db.Ejecutar(@"UPDATE dbo.Turnos
                    SET id_profesional = @profesional, fecha_hora_inicio = @inicio,
                        fecha_hora_fin = @fin, motivo = @motivo
                    WHERE id_turno = @id AND estado = 'pendiente';
                    IF @@ROWCOUNT = 0 THROW 50101, 'El turno ya no está pendiente.', 1;",
                Param("@id", turno.Id), Param("@profesional", turno.ProfesionalId),
                Param("@inicio", turno.Inicio), Param("@fin", turno.Fin), Param("@motivo", turno.Motivo));
        }

        private void Cancelar(int id)
        {
            db.Ejecutar(@"UPDATE dbo.Turnos SET estado = 'cancelado'
                    WHERE id_turno = @id AND estado = 'pendiente';
                    IF @@ROWCOUNT = 0 THROW 50101, 'El turno ya no está pendiente.', 1;",
                Param("@id", id));
        }

        private void Eliminar(int id)
        {
            db.Ejecutar(@"DELETE dbo.Turnos
                    WHERE id_turno = @id AND estado <> 'atendido';
                    IF @@ROWCOUNT = 0 THROW 50102, 'El turno no existe o ya fue atendido.', 1;",
                Param("@id", id));
        }

    
        private DataTable Registrar(int turno, string diagnostico, string tratamiento, string observaciones,
            string comprobante, decimal monto, string metodo)
        {
            var historia = new Dominio.HistoriaClinica
            {
                TurnoId = turno, Diagnostico = diagnostico,
                Tratamiento = tratamiento, Observaciones = observaciones
            };
            var pago = new Dominio.Pago
            {
                TurnoId = turno, NroComprobante = comprobante,
                Monto = monto, MetodoPago = metodo
            };

            return db.Consultar(@"SET XACT_ABORT ON;
                    BEGIN TRANSACTION;
                    IF NOT EXISTS (SELECT 1 FROM dbo.Turnos WHERE id_turno = @turno AND estado <> 'cancelado')
                        THROW 50104, 'El turno no existe o está cancelado.', 1;
                    IF EXISTS (SELECT 1 FROM dbo.HistoriasClinicas WHERE id_turno = @turno)
                        THROW 50105, 'Este turno ya tiene una atención registrada.', 1;
                    EXEC dbo.sp_RegistrarPagoYAtenderTurno
                        @id_turno = @turno, @nro_comprobante = @comprobante,
                        @monto = @monto, @metodo_pago = @metodo;
                    INSERT dbo.HistoriasClinicas(id_turno, diagnostico, tratamiento, observaciones)
                    VALUES(@turno, @diagnostico, @tratamiento, @observaciones);
                    DECLARE @historia INT = CONVERT(INT, SCOPE_IDENTITY());
                    COMMIT TRANSACTION;
                    SELECT @historia Historia,
                        (SELECT id_pago FROM dbo.Pagos WHERE id_turno = @turno) Pago;",
                Param("@turno", historia.TurnoId), Param("@diagnostico", historia.Diagnostico),
                Param("@tratamiento", historia.Tratamiento), Param("@observaciones", historia.Observaciones),
                Param("@comprobante", pago.NroComprobante), Param("@monto", pago.Monto), Param("@metodo", pago.MetodoPago));
        }
    }
}
