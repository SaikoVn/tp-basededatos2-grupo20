# Clínica Veterinaria · Base de Datos II (TPI)

Trabajo Práctico Integrador de **Base de Datos II** — Tecnicatura Universitaria en Programación (UTN - Facultad Regional General Pacheco).

## Puesta en marcha

1. Ejecutar en **SQL Server Management Studio (SSMS)** los scripts de creación y carga en orden numérico (`01_tablas.sql` a `07_datos.sql`). El archivo `08_pruebas_demo.sql` contiene las pruebas por bloques para la demostración en SSMS.
2. Abrir la solución **`BD2-TPI-Veterinaria/AppVeterinaria/AppVeterinaria.sln`** en Visual Studio.
3. Restaurar los paquetes NuGet y compilar la solución. Establecer **`App-Veterinaria-BD2-TPI`** como proyecto de inicio y **`Default.aspx`** como página inicial.
4. En **`Web.config`**, verificar que la cadena de conexión `Veterinaria` apunte a la instancia de SQL Server utilizada en SSMS (por defecto usa `Server=.;Database=Veterinaria;Integrated Security=True`).
5. Ejecutar la aplicación con **IIS Express**.

## Orden de ejecución de los scripts SQL

| Script | Contenido |
| :--- | :--- |
| `01_tablas.sql` | Creación de la base de datos `Veterinaria` y las 8 tablas del modelo |
| `02_constraints.sql` | Restricciones `FOREIGN KEY`, `UNIQUE`, `CHECK`, `DEFAULT` e índice de agenda |
| `03_funciones.sql` | Función escalar `fn_CantidadConsultasMascota` |
| `04_vistas.sql` | Vistas `vw_AgendaTurnosActivos`, `vw_HistorialClinicoPorMascota`, `vw_ResumenFacturacionPorProfesional` y `vw_FrecuenciaConsultasMascotas` |
| `05_procedimientos_almacenados.sql` | Procedimientos `sp_ReporteTurnosProfesional` y `sp_RegistrarPagoYAtenderTurno` |
| `06_triggers.sql` | Triggers `trg_Turnos_AfterInsert`, `trg_Turnos_AfterUpdate` y `trg_Turnos_AfterDelete` |
| `07_datos.sql` | Carga inicial completa (6 especialidades, 8 dueños, 12 mascotas, 6 profesionales, 20 turnos, 8 pagos, 8 historias clínicas, auditoría inicial y bajas lógicas) |
| `08_pruebas_demo.sql` | Pruebas por bloques para el video demostrativo (vistas, UDF, procedimientos, triggers y guía para la app web) |

## Arquitectura y organización del proyecto

- **`AppVeterinaria/Dominio`**: Contiene las **clases de entidad (modelos de datos)** que representan las tablas del sistema mediante propiedades `{ get; set; }` y valores iniciales por defecto (`Duenio.cs`, `Mascota.cs`, `Especialidad.cs`, `Profesional.cs`, `Turno.cs`, `Pago.cs`, `HistoriaClinica.cs` y `AuditoriaTurno.cs`).
- **`AppVeterinaria/Servicio`**: Contiene `AccesoDatos.cs`, que encapsula la conexión a SQL Server (`SqlConnection`, `SqlCommand` y `SqlDataReader`) y provee los métodos `ObtenerTabla`, `EjecutarConsulta`, `EjecutarConsultaEscalar`, `setConsulta`, `setParametro`, `ejecutarLectura`, `ejecutarAccion` y `cerrarConexion` usando ADO.NET y `Microsoft.Data.SqlClient`.
- **`AppVeterinaria/App-Veterinaria-BD2-TPI`**: Proyecto web ASP.NET Web Forms (.NET Framework 4.8). Cada pantalla tiene su vista `.aspx`, su código de eventos `.aspx.cs` y su archivo `.designer.cs`. En los eventos `.aspx.cs` se instancian las entidades de `Dominio`, se arman las listas de parámetros (`List<SqlParameter>`) y se ejecutan las consultas u operaciones a través de `AccesoDatos` (`Servicio`). `Master.Master` define el menú principal y `PaginaClinica.cs` comparte las validaciones de controles y el manejo de errores personalizados de SQL Server (`THROW 50xxx`).
- **`Creacion de DB` / `QUERYS (.SQL)`**: Reúne los scripts SQL enumerados del `01` al `08`.

## Pantallas y objetos SQL

- **Inicio (`Default.aspx`)**: Pantalla principal con accesos directos a la gestión de turnos, consulta de pagos y reporte por profesional.
- **Dueños, Mascotas, Profesionales y Especialidades**: Instancian las entidades `Duenio`, `Mascota`, `Profesional` y `Especialidad` para registrar y editar filas en sus tablas respectivas. La baja lógica de mascotas y la desactivación de profesionales actualizan el campo `activo` verificando previamente que no existan turnos pendientes.
- **Turnos (`Turnos.aspx`)**:
  - Instancia objetos `Turno` para dar de alta, reprogramar, cancelar y eliminar físicamente turnos (cuando están cancelados y sin pagos ni historia clínica), disparando automáticamente los tres triggers del script `06` (`trg_Turnos_AfterInsert`, `trg_Turnos_AfterUpdate` y `trg_Turnos_AfterDelete`).
  - En la acción **Registrar atención**, instancia `HistoriaClinica` y `Pago` y ejecuta dentro de una misma transacción `sp_RegistrarPagoYAtenderTurno` junto con el `INSERT` en `HistoriasClinicas`.
- **Pagos (`Pagos.aspx`)**: Consulta y filtra los pagos registrados en `Pagos` junto con los datos del turno, mascota y profesional.
- **Agenda, Historial, Facturación y Frecuencia**: Consultan respectivamente `vw_AgendaTurnosActivos`, `vw_HistorialClinicoPorMascota`, `vw_ResumenFacturacionPorProfesional` y `vw_FrecuenciaConsultasMascotas` (utilizando `fn_CantidadConsultasMascota` para contabilizar los turnos atendidos).
- **Reporte por profesional (`Reporte.aspx`)**: Ejecuta `sp_ReporteTurnosProfesional` desde el inicio del primer día seleccionado hasta la medianoche posterior al último día (`@hasta_exclusivo`).
- **Auditoría (`Auditoria.aspx`)**: Consulta y filtra `AuditoriaTurnos` por ID de auditoría, ID histórico de turno, tipo de evento (`alta`, `reprogramacion`, `cancelado`, `baja`) y rango de fechas, conservando el ID del turno aun después de su eliminación física.

## Integrantes — Grupo N.º 20

- **Abril Barrientos** (Legajo 29696)
- **Thomas Ferreyra Willemet** (Legajo 33190)
- **Fernando Gabriel Quiroga Medina** (Legajo 33404)
- **Ignacio David Rau** (Legajo 33650)
