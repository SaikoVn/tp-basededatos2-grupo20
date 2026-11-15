
-- Carga completa de datos de prueba para el sistema de Clinica Veterinaria

USE Veterinaria;
GO

-- ============================================================================
-- 1. ESPECIALIDADES (IDs generados: 1 al 6)
-- ============================================================================
INSERT INTO dbo.Especialidades (nombre)
VALUES
    ('Clinica general'),
    ('Dermatologia'),
    ('Traumatologia'),
    ('Cardiologia'),
    ('Cirugia'),
    ('Oftalmologia');
GO

-- ============================================================================
-- 2. DUENIOS (IDs generados: 1 al 8)
-- ============================================================================
INSERT INTO dbo.Duenios (dni, nombre, apellido, telefono, email, direccion)
VALUES
    ('35111222', 'Ana',      'Lopez',     '1144441111', 'ana.lopez@mail.com',      'Av. Hipolito Yrigoyen 1200, Gral. Pacheco'),
    ('38333444', 'Luis',     'Perez',     '1144442222', 'luis.perez@mail.com',     'Av. Cazon 450, Tigre'),
    ('41555666', 'Sofia',    'Martinez',  '1144443333', 'sofia.martinez@mail.com', 'Av. Constituyentes 800, Don Torcuato'),
    ('32777888', 'Carlos',   'Gomez',     '1144444444', 'carlos.gomez@mail.com',   'Ruta 27 KM 5, Benavidez'),
    ('40999000', 'Lucia',    'Fernandez', '1144445555', 'lucia.f@mail.com',        'Calle Boulogne Sur Mer 320, Gral. Pacheco'),
    ('29123456', 'Martin',   'Diaz',      '1144446666', 'martin.diaz@mail.com',    'Av. Libertador 2100, San Fernando'),
    ('43654321', 'Valentina','Romero',    '1144447777', 'valen.romero@mail.com',   'Calle Alvear 150, Martinez'),
    ('36987654', 'Diego',    'Torres',    '1144448888', 'diego.torres@mail.com',   'Calle Belgrano 940, Don Torcuato');
GO

-- ============================================================================
-- 3. MASCOTAS (IDs generados: 1 al 12)
-- El campo activo se inicia en 1 por su restriccion DEFAULT.
-- Se incluye un caso con sexo NULL (permitido por el modelo cuando se desconoce).
-- ============================================================================
INSERT INTO dbo.Mascotas (id_duenio, nombre, especie, raza, fecha_nacimiento, sexo)
VALUES
    (1, 'Milo',   'Perro',  'Mestizo',          '2021-05-10', 'M'), -- ID 1 (Ana)
    (2, 'Luna',   'Gato',   'Siames',           '2022-08-15', 'F'), -- ID 2 (Luis)
    (2, 'Nube',   'Perro',  'Caniche',          '2019-03-20', 'F'), -- ID 3 (Luis - luego se da de baja)
    (3, 'Toby',   'Perro',  'Labrador',         '2023-01-12', 'M'), -- ID 4 (Sofia)
    (4, 'Simba',  'Gato',   'Comun europeo',    '2020-11-05', 'M'), -- ID 5 (Carlos)
    (5, 'Lola',   'Perro',  'Bulldog Frances',  '2022-04-22', 'F'), -- ID 6 (Lucia)
    (6, 'Rocky',  'Perro',  'Ovejero Aleman',   '2018-09-30', 'M'), -- ID 7 (Martin)
    (7, 'Mia',    'Gato',   'Persa',            '2023-06-18', 'F'), -- ID 8 (Valentina)
    (8, 'Coco',   'Conejo', 'Cabeza de leon',   '2024-02-10', NULL),-- ID 9 (Diego - sexo sin especificar)
    (4, 'Rocco',  'Perro',  'Boxer',            '2017-12-01', 'M'), -- ID 10 (Carlos - luego se da de baja)
    (1, 'Kira',   'Gato',   'Mestizo',          '2021-10-14', 'F'), -- ID 11 (Ana)
    (5, 'Max',    'Perro',  'Golden Retriever', '2020-07-08', 'M'); -- ID 12 (Lucia)
GO

-- ============================================================================
-- 4. PROFESIONALES (IDs generados: 1 al 6)
-- Los primeros 5 estan activos (1) y la profesional 6 esta inactiva (0)
-- para probar el rechazo del trigger cuando se intenta asignarle un turno.
-- ============================================================================
INSERT INTO dbo.Profesionales (id_especialidad, matricula, nombre, apellido, telefono, email, activo)
VALUES
    (1, 'MP-1001', 'Eva',     'Garcia',  '1155551111', 'eva.garcia@vet.com',     1), -- ID 1: Clinica general
    (2, 'MP-1002', 'Pablo',   'Lopez',   '1155552222', 'pablo.lopez@vet.com',    1), -- ID 2: Dermatologia
    (3, 'MP-1003', 'Mariano', 'Suarez',  '1155553333', 'mariano.suarez@vet.com', 1), -- ID 3: Traumatologia
    (4, 'MP-1004', 'Laura',   'Benitez', '1155554444', 'laura.benitez@vet.com',  1), -- ID 4: Cardiologia
    (5, 'MP-1005', 'Nicolas', 'Vera',    '1155555555', 'nicolas.vera@vet.com',   1), -- ID 5: Cirugia (sin cobros aun, mostrara $0 en la vista)
    (6, 'MP-1006', 'Carla',   'Gomez',   '1155556666', 'carla.gomez@vet.com',    0); -- ID 6: Oftalmologia (INACTIVA)
GO

-- ============================================================================
-- 5. TURNOS (IDs generados: 1 al 20)
-- Cada INSERT dispara trg_Turnos_AfterInsert y registra 'alta' en AuditoriaTurnos.
-- ============================================================================
INSERT INTO dbo.Turnos (id_mascota, id_profesional, fecha_hora_inicio, fecha_hora_fin, estado, motivo)
VALUES
    -- BLOQUE A: Turnos pasados ya atendidos (IDs 1 al 8)
    -- Permiten ver datos completos en Historial, Pagos, Facturacion y Frecuencia:
    (1,  1, '2026-10-15T09:00:00', '2026-10-15T09:30:00', 'atendido',  'Control general de ingreso'),         -- ID 1 (Milo - consulta 1)
    (1,  1, '2026-11-02T10:00:00', '2026-11-02T10:30:00', 'atendido',  'Refuerzo de vacunacion'),             -- ID 2 (Milo - consulta 2)
    (2,  2, '2026-11-03T11:00:00', '2026-11-03T11:30:00', 'atendido',  'Caida de pelo localizada'),           -- ID 3 (Luna - consulta 1)
    (3,  2, '2026-11-04T09:30:00', '2026-11-04T10:00:00', 'atendido',  'Alergia cutanea en orejas'),          -- ID 4 (Nube - luego inactiva)
    (4,  3, '2026-11-05T14:00:00', '2026-11-05T14:30:00', 'atendido',  'Renguera en pata trasera derecha'),   -- ID 5 (Toby - consulta 1)
    (5,  4, '2026-11-06T15:00:00', '2026-11-06T15:30:00', 'atendido',  'Chequeo cardiologico prequirurgico'), -- ID 6 (Simba - consulta 1)
    (6,  1, '2026-11-07T09:00:00', '2026-11-07T09:30:00', 'atendido',  'Control digestivo y peso'),           -- ID 7 (Lola - consulta 1)
    (10, 3, '2026-11-08T10:30:00', '2026-11-08T11:00:00', 'atendido',  'Control articular de cadera'),        -- ID 8 (Rocco - luego inactivo)

    -- BLOQUE B: Turnos para generar movimientos iniciales en AuditoriaTurnos (IDs 9 al 11)
    -- Se modifican/eliminan mas abajo en este mismo script para que Auditoria ya tenga los 4 eventos:
    (7,  1, '2026-11-18T09:00:00', '2026-11-18T09:30:00', 'pendiente', 'Control anual de Rocky'),             -- ID 9 (se reprograma abajo)
    (8,  2, '2026-11-18T10:00:00', '2026-11-18T10:30:00', 'pendiente', 'Revision de pelaje de Mia'),          -- ID 10 (se cancela abajo)
    (9,  1, '2026-11-18T11:00:00', '2026-11-18T11:30:00', 'pendiente', 'Turno duplicado por error'),          -- ID 11 (se elimina abajo con DELETE)

    -- BLOQUE C: Turnos pendientes para usar en el Script 08 en SSMS (IDs 12 al 16)
    (1,  1, '2026-11-20T09:00:00', '2026-11-20T09:30:00', 'pendiente', 'Control trimestral (SSMS: Pagar con SP)'),     -- ID 12
    (2,  1, '2026-11-20T10:00:00', '2026-11-20T10:30:00', 'pendiente', 'Control de peso (SSMS: Reprogramar)'),         -- ID 13
    (5,  2, '2026-11-20T11:00:00', '2026-11-20T11:30:00', 'pendiente', 'Control de piel (SSMS: Cancelar)'),            -- ID 14
    (11, 3, '2026-11-20T12:00:00', '2026-11-20T12:30:00', 'pendiente', 'Turno de prueba (SSMS: Eliminar con DELETE)'), -- ID 15
    (3,  2, '2026-11-21T09:00:00', '2026-11-21T09:30:00', 'pendiente', 'Seguimiento Nube (SSMS: Mascota inactiva)'),   -- ID 16

    -- BLOQUE D: Turnos pendientes reservados para demostrar la Aplicacion Web en el video (IDs 17 al 20)
    (4,  1, '2026-11-22T09:00:00', '2026-11-22T09:30:00', 'pendiente', 'Consulta general (Web: Registrar atencion)'),  -- ID 17
    (6,  2, '2026-11-22T10:00:00', '2026-11-22T10:30:00', 'pendiente', 'Chequeo alergia (Web: Reprogramar)'),          -- ID 18
    (12, 4, '2026-11-22T11:00:00', '2026-11-22T11:30:00', 'pendiente', 'Control cardiaco (Web: Cancelar y Eliminar)'), -- ID 19
    (7,  5, '2026-11-23T09:30:00', '2026-11-23T10:00:00', 'pendiente', 'Evaluacion prequirurgica en agenda');          -- ID 20
GO

-- ============================================================================
-- 6. PAGOS DE LOS TURNOS ATENDIDOS (Turnos 1 al 8)
-- Se cubren los tres metodos permitidos: efectivo, tarjeta y transferencia.
-- ============================================================================
INSERT INTO dbo.Pagos (id_turno, nro_comprobante, monto, metodo_pago, fecha_pago)
VALUES
    (1, 'COMP-0001', 15000.00, 'efectivo',      '2026-10-15T09:35:00'),
    (2, 'COMP-0002', 16500.00, 'tarjeta',       '2026-11-02T10:35:00'),
    (3, 'COMP-0003', 18000.00, 'transferencia', '2026-11-03T11:35:00'),
    (4, 'COMP-0004', 18500.00, 'efectivo',      '2026-11-04T10:05:00'),
    (5, 'COMP-0005', 22000.00, 'tarjeta',       '2026-11-05T14:35:00'),
    (6, 'COMP-0006', 25000.00, 'transferencia', '2026-11-06T15:35:00'),
    (7, 'COMP-0007', 15500.00, 'efectivo',      '2026-11-07T09:35:00'),
    (8, 'COMP-0008', 21000.00, 'tarjeta',       '2026-11-08T11:05:00');
GO

-- ============================================================================
-- 7. HISTORIAS CLINICAS DE LOS TURNOS ATENDIDOS (Turnos 1 al 8)
-- ============================================================================
INSERT INTO dbo.HistoriasClinicas (id_turno, diagnostico, tratamiento, observaciones, fecha_registro)
VALUES
    (1, 'Paciente sano y en peso adecuado',            'Desparasitacion interna',                 'Control anual sin novedades',                  '2026-10-15T09:32:00'),
    (2, 'Apto para vacunacion anual',                  'Aplicacion de vacuna sextuple y rabia',   'Sin reacciones adversas',                      '2026-11-02T10:32:00'),
    (3, 'Dermatitis alergica por pulgas',              'Pipeta antiparasitaria y locion topica',  'Revisar evolucion en 15 dias',                 '2026-11-03T11:32:00'),
    (4, 'Otitis externa y dermatitis leve',            'Gotas oticas cada 12 hs por 7 dias',      'Historia conservada de mascota luego inactiva','2026-11-04T10:02:00'),
    (5, 'Distension muscular leve en miembro derecho', 'Antiinflamatorio oral por 5 dias',        'Reposo relativo sin correr',                   '2026-11-05T14:32:00'),
    (6, 'Ritmo cardiaco normal, apto quirurgico',      'Sin medicacion cardiologica',             NULL,                                           '2026-11-06T15:32:00'),
    (7, 'Gastritis leve por cambio de alimento',       'Dieta blanda por 72 horas y protector',   'Volver si persisten vomitos',                  '2026-11-07T09:32:00'),
    (8, 'Desgaste articular propio de la edad',        'Condroprotector diario por 30 dias',      'Control de paciente geronte',                  '2026-11-08T11:02:00');
GO

-- ============================================================================
-- 8. MOVIMIENTOS INICIALES PARA DEJAR LA AUDITORIA Y BAJAS LOGICAS LISTAS
-- ============================================================================

-- 8.1. Reprogramamos el Turno 9 (dispara trg_Turnos_AfterUpdate -> 'reprogramacion')
UPDATE dbo.Turnos
SET fecha_hora_inicio = '2026-11-18T16:00:00',
    fecha_hora_fin    = '2026-11-18T16:30:00'
WHERE id_turno = 9;
GO

-- 8.2. Cancelamos el Turno 10 (dispara trg_Turnos_AfterUpdate -> 'cancelado')
UPDATE dbo.Turnos
SET estado = 'cancelado'
WHERE id_turno = 10;
GO

-- 8.3. Eliminamos fisicamente el Turno 11 (dispara trg_Turnos_AfterDelete -> 'baja')
-- Conserva id_turno = 11 en AuditoriaTurnos como referencia historica sin FK.
DELETE FROM dbo.Turnos
WHERE id_turno = 11;
GO

-- 8.4. Baja logica de las mascotas Nube (ID 3) y Rocco (ID 10)
-- Sus historias clinicas (Turnos 4 y 8) siguen disponibles en vw_HistorialClinicoPorMascota.
UPDATE dbo.Mascotas
SET activo = 0
WHERE id_mascota IN (3, 10);
GO

-- ============================================================================
-- 9. CONSULTAS DE VERIFICACION DE LA CARGA INICIAL
-- ============================================================================
SELECT * FROM dbo.Especialidades;
SELECT * FROM dbo.Duenios;
SELECT * FROM dbo.Mascotas;
SELECT * FROM dbo.Profesionales;
SELECT * FROM dbo.Turnos;
SELECT * FROM dbo.Pagos;
SELECT * FROM dbo.HistoriasClinicas;
SELECT * FROM dbo.AuditoriaTurnos;
GO