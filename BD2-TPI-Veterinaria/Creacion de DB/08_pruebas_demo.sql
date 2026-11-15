
-- Script demostrativo para el video del TPI (SSMS + Guia para la App Web)

USE Veterinaria;
GO

-- ============================================================================
-- PARTE 1: VISTAS Y FUNCION DEFINIDA POR EL USUARIO (UDF)
-- ============================================================================

-- 1.1. Probamos la funcion escalar que cuenta turnos atendidos de una mascota
-- Milo (id_mascota = 1) ya tiene 2 consultas atendidas cargadas en el script 07:
SELECT
    dbo.fn_CantidadConsultasMascota(1) AS consultas_atendidas_milo,
    dbo.fn_CantidadConsultasMascota(2) AS consultas_atendidas_luna,
    dbo.fn_CantidadConsultasMascota(7) AS consultas_atendidas_rocky;
GO

-- 1.2. Vista 1: Agenda de turnos de mascotas activas
-- (Muestra los turnos de mascotas con activo = 1; excluye a Nube y Rocco que estan inactivos)
SELECT * FROM dbo.vw_AgendaTurnosActivos
ORDER BY fecha_hora_inicio;
GO

-- 1.3. Vista 2: Historial clinico por mascota
-- (Muestra las 8 historias clinicas, incluyendo las de Nube y Rocco aunque esten dados de baja)
SELECT * FROM dbo.vw_HistorialClinicoPorMascota
ORDER BY fecha_registro;
GO

-- 1.4. Vista 3: Resumen de facturacion por profesional
-- (Muestra cantidad de pagos y monto total por profesional; Nicolas y Carla muestran 0 gracias a ISNULL)
SELECT * FROM dbo.vw_ResumenFacturacionPorProfesional
ORDER BY id_profesional;
GO

-- 1.5. Vista 4: Frecuencia de consultas por mascota (utiliza internamente la UDF)
SELECT * FROM dbo.vw_FrecuenciaConsultasMascotas
ORDER BY total_consultas_atendidas DESC, id_mascota;
GO


-- ============================================================================
-- PARTE 2: PROCEDIMIENTOS ALMACENADOS
-- ============================================================================

-- 2.1. Procedimiento 1: Reporte de turnos de un profesional entre dos fechas
-- Consultamos todos los turnos de Eva Garcia (id_profesional = 1) en noviembre 2026:
EXEC dbo.sp_ReporteTurnosProfesional
    @id_profesional = 1,
    @desde = '2026-11-01T00:00:00',
    @hasta_exclusivo = '2026-12-01T00:00:00';
GO

-- Validacion del Procedimiento 1: si @desde >= @hasta_exclusivo, devuelve error 50001
BEGIN TRY
    EXEC dbo.sp_ReporteTurnosProfesional
        @id_profesional = 1,
        @desde = '2026-11-30T00:00:00',
        @hasta_exclusivo = '2026-11-01T00:00:00';
END TRY
BEGIN CATCH
    PRINT 'Error esperado en sp_ReporteTurnosProfesional: ' + ERROR_MESSAGE();
END CATCH;
GO

-- 2.2. Procedimiento 2: Registrar pago y cambiar el turno a estado 'atendido'
-- Consultamos el Turno 12 antes del cobro (esta en estado 'pendiente'):
SELECT id_turno, id_mascota, id_profesional, estado, motivo
FROM dbo.Turnos
WHERE id_turno = 12;

-- Ejecutamos el procedimiento para cobrar el Turno 12:
EXEC dbo.sp_RegistrarPagoYAtenderTurno
    @id_turno = 12,
    @nro_comprobante = 'COMP-0009',
    @monto = 17500.00,
    @metodo_pago = 'tarjeta';

-- Comprobamos que se inserto el pago, el Turno 12 paso a 'atendido' y subio la facturacion de Eva:
SELECT id_turno, id_mascota, id_profesional, estado, motivo FROM dbo.Turnos WHERE id_turno = 12;
SELECT * FROM dbo.Pagos WHERE id_turno = 12;
SELECT * FROM dbo.vw_ResumenFacturacionPorProfesional WHERE id_profesional = 1;
GO

-- Validaciones del Procedimiento 2 (casos rechazados por reglas de negocio):
-- a) Intentar pagar un turno inexistente (ID 999):
BEGIN TRY
    EXEC dbo.sp_RegistrarPagoYAtenderTurno
        @id_turno = 999,
        @nro_comprobante = 'COMP-ERR-01',
        @monto = 15000.00,
        @metodo_pago = 'efectivo';
END TRY
BEGIN CATCH
    PRINT 'Error esperado (turno inexistente): ' + ERROR_MESSAGE();
END CATCH;

-- b) Intentar pagar por segunda vez el mismo turno (Turno 12):
BEGIN TRY
    EXEC dbo.sp_RegistrarPagoYAtenderTurno
        @id_turno = 12,
        @nro_comprobante = 'COMP-ERR-02',
        @monto = 17500.00,
        @metodo_pago = 'efectivo';
END TRY
BEGIN CATCH
    PRINT 'Error esperado (turno ya pagado): ' + ERROR_MESSAGE();
END CATCH;

-- c) Intentar pagar un turno cancelado (Turno 10):
BEGIN TRY
    EXEC dbo.sp_RegistrarPagoYAtenderTurno
        @id_turno = 10,
        @nro_comprobante = 'COMP-ERR-03',
        @monto = 15000.00,
        @metodo_pago = 'efectivo';
END TRY
BEGIN CATCH
    PRINT 'Error esperado (turno cancelado): ' + ERROR_MESSAGE();
END CATCH;

-- d) Intentar pagar el turno de una mascota dada de baja (Turno 16 de Nube):
BEGIN TRY
    EXEC dbo.sp_RegistrarPagoYAtenderTurno
        @id_turno = 16,
        @nro_comprobante = 'COMP-ERR-04',
        @monto = 15000.00,
        @metodo_pago = 'efectivo';
END TRY
BEGIN CATCH
    PRINT 'Error esperado (mascota inactiva): ' + ERROR_MESSAGE();
END CATCH;
GO


-- ============================================================================
-- PARTE 3: TRIGGERS SOBRE LA TABLA TURNOS (INSERT, UPDATE, DELETE)
-- ============================================================================

-- 3.1. Trigger AFTER INSERT (trg_Turnos_AfterInsert)
-- a) Caso valido: insertamos un nuevo turno (Turno 21) y el trigger registra el evento 'alta'
INSERT INTO dbo.Turnos (id_mascota, id_profesional, fecha_hora_inicio, fecha_hora_fin, motivo)
VALUES (1, 2, '2026-11-24T09:00:00', '2026-11-24T09:30:00', 'Consulta dermatologica nueva');

SELECT * FROM dbo.AuditoriaTurnos WHERE id_turno = 21;
GO

-- b) Validacion Trigger INSERT: rechaza turno superpuesto para el mismo profesional
BEGIN TRY
    INSERT INTO dbo.Turnos (id_mascota, id_profesional, fecha_hora_inicio, fecha_hora_fin, motivo)
    VALUES (2, 2, '2026-11-24T09:15:00', '2026-11-24T09:45:00', 'Turno superpuesto con el anterior');
END TRY
BEGIN CATCH
    PRINT 'Error esperado (turno superpuesto en INSERT): ' + ERROR_MESSAGE();
END CATCH;

-- c) Validacion Trigger INSERT: rechaza turno con profesional inactivo (Carla = ID 6)
BEGIN TRY
    INSERT INTO dbo.Turnos (id_mascota, id_profesional, fecha_hora_inicio, fecha_hora_fin, motivo)
    VALUES (1, 6, '2026-11-25T10:00:00', '2026-11-25T10:30:00', 'Turno con profesional inactivo');
END TRY
BEGIN CATCH
    PRINT 'Error esperado (profesional inactivo en INSERT): ' + ERROR_MESSAGE();
END CATCH;
GO

-- 3.2. Trigger AFTER UPDATE (trg_Turnos_AfterUpdate)
-- a) Reprogramar el Turno 13 (cambia de las 10:00 a las 15:00 del mismo dia):
SELECT id_turno, fecha_hora_inicio, fecha_hora_fin, estado, motivo FROM dbo.Turnos WHERE id_turno = 13;

UPDATE dbo.Turnos
SET fecha_hora_inicio = '2026-11-20T15:00:00',
    fecha_hora_fin    = '2026-11-20T15:30:00'
WHERE id_turno = 13;

-- b) Cancelar el Turno 14 (pasa de 'pendiente' a 'cancelado'):
UPDATE dbo.Turnos
SET estado = 'cancelado'
WHERE id_turno = 14;

-- Verificamos los cambios en Turnos y en AuditoriaTurnos:
SELECT id_turno, fecha_hora_inicio, fecha_hora_fin, estado, motivo FROM dbo.Turnos WHERE id_turno IN (13, 14);
SELECT * FROM dbo.AuditoriaTurnos WHERE id_turno IN (13, 14) ORDER BY id_auditoria;
GO

-- c) Validacion Trigger UPDATE: rechaza reprogramar un turno si se pisa con otro del mismo profesional
-- (Intentamos mover el Turno 13 al horario de las 09:00 del 20/11 que ya ocupa el Turno 12 de Eva)
BEGIN TRY
    UPDATE dbo.Turnos
    SET fecha_hora_inicio = '2026-11-20T09:00:00',
        fecha_hora_fin    = '2026-11-20T09:30:00'
    WHERE id_turno = 13;
END TRY
BEGIN CATCH
    PRINT 'Error esperado (superposicion al reprogramar en UPDATE): ' + ERROR_MESSAGE();
END CATCH;
GO

-- 3.3. Trigger AFTER DELETE (trg_Turnos_AfterDelete)
-- Eliminamos fisicamente el Turno 15 (que no posee pago ni historia clinica):
SELECT * FROM dbo.Turnos WHERE id_turno = 15;

DELETE FROM dbo.Turnos
WHERE id_turno = 15;

-- Verificamos que ya no esta en Turnos, pero su ID 15 sigue auditado con tipo_evento = 'baja':
SELECT * FROM dbo.Turnos WHERE id_turno = 15;
SELECT * FROM dbo.AuditoriaTurnos WHERE id_turno = 15;
GO

-- 3.4. Consulta completa de AuditoriaTurnos (muestra alta, reprogramacion, cancelado y baja)
SELECT * FROM dbo.AuditoriaTurnos
ORDER BY id_auditoria;
GO

/*
===============================================================================
PARTE 4: GUIA PASO A PASO PARA DEMOSTRAR LA APLICACION WEB EN EL VIDEO
===============================================================================
Los Turnos 17, 18, 19 y 20 quedaron listos en estado 'pendiente' para la Web:

1. PANTALLAS DE VISTAS Y UDF:
   - "Agenda": consulta vw_AgendaTurnosActivos (filtrar por mascota o profesional).
   - "Historial": consulta vw_HistorialClinicoPorMascota (filtrar por "Nube" o
     "Rocco" para mostrar que su historial clinico se mantiene aunque esten dados de baja).
   - "Facturacion": consulta vw_ResumenFacturacionPorProfesional (mostrar los cobros
     acumulados y que Nicolas Vera y Carla Gomez figuran con $0).
   - "Frecuencia": consulta vw_FrecuenciaConsultasMascotas (Milo ya aparece primero
     con 3 consultas atendidas tras cobrar el Turno 12).

2. PANTALLA DE REPORTE (PROCEDIMIENTO 1):
   - "Reporte por profesional": seleccionar a "Garcia, Eva · MP-1001", desde
     01/11/2026 hasta 30/11/2026 y hacer clic en "Consultar".

3. ATENCION Y COBRO EN TRANSACCION (PROCEDIMIENTO 2 + HISTORIA CLINICA):
   - Ir a "Turnos" y en el Turno 17 (Toby) hacer clic en "Registrar atencion".
   - Cargar Diagnostico, Tratamiento, Comprobante "COMP-0010", Monto "20000" y
     Metodo "Transferencia", y guardar.
   - Mostrar en "Pagos" y en "Historial" el nuevo registro creado.

4. TRIGGERS Y AUDITORIA EN VIVO DESDE LA WEB:
   - En "Turnos":
     * Intentar dar de alta un turno superpuesto en el mismo horario -> mostrar la
       alerta roja devuelta por el trigger (Error 50012).
     * En el Turno 18 (Lola), hacer clic en "Reprogramar" y cambiar el horario.
     * En el Turno 19 (Max), hacer clic en "Cancelar" y luego en "Eliminar".
   - Ir a "Auditoria" y filtrar por Turno "19" para mostrar en pantalla sus eventos
     de alta, cancelado y baja conservados como referencia historica.
===============================================================================
*/