# Clínica Veterinaria · Base de Datos II (TPI)

Trabajo Práctico Integrador de **Base de Datos II** — Tecnicatura Universitaria en Programación, UTN Facultad Regional General Pacheco.

## Requisitos

- SQL Server y SQL Server Management Studio (SSMS).
- Visual Studio con soporte para ASP.NET Web Forms y .NET Framework 4.8.
- IIS Express, disponible desde Visual Studio.

## Puesta en marcha

### 1. Crear y cargar la base de datos

En SSMS, ejecutar una sola vez y en orden los scripts de la carpeta `Creacion de DB`:

1. `01_tablas.sql`
2. `02_constraints.sql`
3. `03_funciones.sql`
4. `04_vistas.sql`
5. `05_procedimientos_almacenados.sql`
6. `06_triggers.sql`
7. `07_datos.sql`

El primer script crea la base `Veterinaria` si todavía no existe y luego crea sus tablas. Los scripts siguientes agregan objetos y datos; por eso, no se deben volver a ejecutar sobre una base ya inicializada. `07_datos.sql` carga datos de ejemplo para usar la aplicación y las demostraciones.

`08_pruebas_demo.sql` es opcional. Ejecutarlo después del script `07`, sobre datos de demostración, y una sola vez: realiza operaciones que modifican la base, como registrar un pago, reprogramar y cancelar turnos y eliminar uno físicamente.

### 2. Configurar la conexión

La cadena de conexión está en `AppVeterinaria/App-Veterinaria-BD2-TPI/Web.config`, bajo el nombre `Veterinaria`. Por defecto apunta a:

```text
Server=.;Database=Veterinaria;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=5
```

Si SQL Server está en otra instancia, cambiar `Server=.` por el nombre correspondiente. 

### 3. Abrir y ejecutar la aplicación

1. Abrir `AppVeterinaria/AppVeterinaria.sln` en Visual Studio.
2. Restaurar los paquetes NuGet y compilar la solución.
3. Establecer `App-Veterinaria-BD2-TPI` como proyecto de inicio.
4. Ejecutar con IIS Express. `Default.aspx` está configurada como página predeterminada.

## Scripts y objetos de base de datos

| Script | Contenido |
| --- | --- |
| `01_tablas.sql` | Crea la base `Veterinaria` y las tablas `Duenios`, `Especialidades`, `Mascotas`, `Profesionales`, `Turnos`, `Pagos`, `HistoriasClinicas` y `AuditoriaTurnos`. |
| `02_constraints.sql` | Agrega claves foráneas, restricciones `UNIQUE`, `CHECK` y `DEFAULT`, e índices para las consultas de turnos. |
| `03_funciones.sql` | Define `fn_CantidadConsultasMascota`. |
| `04_vistas.sql` | Define `vw_AgendaTurnosActivos`, `vw_HistorialClinicoPorMascota`, `vw_ResumenFacturacionPorProfesional` y `vw_FrecuenciaConsultasMascotas`. |
| `05_procedimientos_almacenados.sql` | Define `sp_ReporteTurnosProfesional` y `sp_RegistrarPagoYAtenderTurno`. |
| `06_triggers.sql` | Define triggers para altas, modificaciones y bajas físicas de turnos; también registra eventos de auditoría. |
| `07_datos.sql` | Carga datos iniciales de ejemplo. |
| `08_pruebas_demo.sql` | Contiene consultas y operaciones de demostración para vistas, procedimientos, triggers y auditoría. |

## Arquitectura de la aplicación

La solución separa la interfaz web, los modelos de dominio y el acceso a datos:

- **`AppVeterinaria/Dominio`**: clases de entidad con propiedades para representar los datos del sistema: `Duenio`, `Mascota`, `Especialidad`, `Profesional`, `Turno`, `Pago` e `HistoriaClinica`.
- **`AppVeterinaria/Servicio`**: `AccesoDatos.cs` centraliza el acceso a SQL Server con ADO.NET y `Microsoft.Data.SqlClient`. Expone `Consultar`, `Ejecutar` y `Param` para trabajar con consultas parametrizadas.
- **`AppVeterinaria/App-Veterinaria-BD2-TPI`**: aplicación ASP.NET Web Forms sobre .NET Framework 4.8. Las páginas `.aspx` presentan la interfaz y sus archivos `.aspx.cs` gestionan eventos y operaciones. El código de las páginas utiliza las entidades de `Dominio` y delega las consultas y escrituras en `AccesoDatos`.
- **`PaginaClinica.cs`**: reúne validaciones y manejo de errores de SQL Server utilizados por las páginas.

## Pantallas y funciones

- **Inicio (`Default.aspx`)**: accesos a la gestión de turnos, pagos y reportes.
- **Dueños, Mascotas, Profesionales y Especialidades**: permiten consultar y gestionar los registros correspondientes. Las mascotas pueden darse de baja lógicamente y los profesionales desactivarse, con validaciones de turnos pendientes.
- **Turnos (`Turnos.aspx`)**: permite dar de alta, reprogramar y cancelar turnos, además de eliminar físicamente turnos cancelados que no tengan pagos ni historia clínica. Los triggers registran los eventos de auditoría y validan reglas como la superposición de horarios.
- **Registrar atención**: registra el pago, marca el turno como atendido e inserta la historia clínica dentro de una misma transacción.
- **Pagos (`Pagos.aspx`)**: consulta y filtra pagos junto con información del turno, la mascota y el profesional.
- **Agenda (`Agenda.aspx`)**: consulta la vista `vw_AgendaTurnosActivos`.
- **Historial (`Historial.aspx`)**: consulta `vw_HistorialClinicoPorMascota`.
- **Facturación (`Facturacion.aspx`)**: consulta `vw_ResumenFacturacionPorProfesional`.
- **Frecuencia (`Frecuencia.aspx`)**: consulta `vw_FrecuenciaConsultasMascotas`, que utiliza `fn_CantidadConsultasMascota`.
- **Reporte por profesional (`Reporte.aspx`)**: ejecuta `sp_ReporteTurnosProfesional`. El día final seleccionado se incluye usando como límite exclusivo la medianoche del día siguiente.
- **Auditoría (`Auditoria.aspx`)**: permite consultar y filtrar los eventos de alta, reprogramación, cancelación y baja. La auditoría conserva el identificador histórico del turno aunque este se elimine físicamente.

## Integrantes — Grupo N.º 20

- **Abril Barrientos** (Legajo 29696)
- **Thomas Ferreyra Willemet** (Legajo 33190)
- **Fernando Gabriel Quiroga Medina** (Legajo 33404)
- **Ignacio David Rau** (Legajo 33650)
