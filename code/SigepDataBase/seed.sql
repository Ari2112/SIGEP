-- ============================================
-- SIGEP - Datos Iniciales (Seed)
-- SQL Server
-- ============================================

-- ============================================
-- CATÁLOGOS BASE
-- ============================================

-- Horarios
SET IDENTITY_INSERT Schedules ON;
INSERT INTO Schedules (Id, Name, StartTime, EndTime, WorkHoursPerDay, IsActive)
VALUES 
    (1, 'Jornada Completa 8:00-17:00', '08:00:00', '17:00:00', 8, 1),
    (2, 'Jornada Matutina 6:00-14:00', '06:00:00', '14:00:00', 8, 1),
    (3, 'Jornada Vespertina 14:00-22:00', '14:00:00', '22:00:00', 8, 1),
    (4, 'Jornada Nocturna 22:00-6:00', '22:00:00', '06:00:00', 8, 1),
    (5, 'Media Jornada Mañana 8:00-12:00', '08:00:00', '12:00:00', 4, 1);
SET IDENTITY_INSERT Schedules OFF;

-- Posiciones/Cargos
SET IDENTITY_INSERT Positions ON;
INSERT INTO Positions (Id, Name, Description, BaseSalary, IsActive)
VALUES 
    (1, 'Gerente General', 'Gerente de la empresa', 1500000.00, 1),
    (2, 'Gerente RRHH', 'Gerente de Recursos Humanos', 1200000.00, 1),
    (3, 'Supervisor', 'Supervisor de área', 800000.00, 1),
    (4, 'Analista', 'Analista general', 600000.00, 1),
    (5, 'Asistente', 'Asistente administrativo', 450000.00, 1),
    (6, 'Desarrollador', 'Desarrollador de software', 700000.00, 1),
    (7, 'Contador', 'Contador general', 650000.00, 1),
    (8, 'Recepcionista', 'Recepcionista', 400000.00, 1),
    (9, 'Conserje', 'Personal de limpieza y mantenimiento', 350000.00, 1);
SET IDENTITY_INSERT Positions OFF;

-- Tipos de Permisos
SET IDENTITY_INSERT PermissionTypes ON;
INSERT INTO PermissionTypes (Id, Name, Description, MaxDaysPerYear, RequiresDocument, IsPaid, IsActive)
VALUES 
    (1, 'Personal', 'Permiso por asuntos personales', 5, 0, 1, 1),
    (2, 'Médico', 'Cita médica o procedimiento', 12, 1, 1, 1),
    (3, 'Familiar', 'Asuntos familiares urgentes', 3, 0, 1, 1),
    (4, 'Estudio', 'Permisos para estudios o exámenes', 10, 1, 0, 1),
    (5, 'Matrimonio', 'Permiso por matrimonio', 3, 1, 1, 1),
    (6, 'Duelo', 'Fallecimiento de familiar', 5, 1, 1, 1),
    (7, 'Nacimiento', 'Nacimiento de hijo', 3, 1, 1, 1),
    (8, 'Judicial', 'Citación judicial o legal', NULL, 1, 1, 1),
    (9, 'Otro', 'Otros permisos', NULL, 0, 0, 1);
SET IDENTITY_INSERT PermissionTypes OFF;

-- Tipos de Deducciones
SET IDENTITY_INSERT DeductionTypes ON;
INSERT INTO DeductionTypes (Id, Name, Description, IsPercentage, DefaultValue, IsActive)
VALUES 
    (1, 'CCSS Trabajador', 'Cuota obrero CCSS', 1, 0.1067, 1),
    (2, 'Impuesto Renta', 'Impuesto sobre la renta', 1, 0.00, 1),
    (3, 'Embargo Judicial', 'Embargo por orden judicial', 0, 0.00, 1),
    (4, 'Préstamo Empresa', 'Préstamo otorgado por la empresa', 0, 0.00, 1),
    (5, 'Asociación Solidarista', 'Aporte a asociación solidarista', 1, 0.05, 1),
    (6, 'Pensión Alimenticia', 'Pensión alimenticia', 0, 0.00, 1),
    (7, 'Adelanto Salario', 'Adelanto de salario', 0, 0.00, 1);
SET IDENTITY_INSERT DeductionTypes OFF;

-- Tipos de Beneficios
SET IDENTITY_INSERT BenefitTypes ON;
INSERT INTO BenefitTypes (Id, Name, Description, IsPercentage, DefaultValue, IsActive)
VALUES 
    (1, 'Bono Alimentación', 'Bono para alimentación', 0, 50000.00, 1),
    (2, 'Bono Transporte', 'Subsidio de transporte', 0, 30000.00, 1),
    (3, 'Comisiones', 'Comisiones por ventas', 0, 0.00, 1),
    (4, 'Bonificación', 'Bonificación especial', 0, 0.00, 1),
    (5, 'Retroactivo', 'Pago retroactivo', 0, 0.00, 1);
SET IDENTITY_INSERT BenefitTypes OFF;

-- ============================================
-- EMPLEADOS
-- ============================================

SET IDENTITY_INSERT Employees ON;
INSERT INTO Employees (Id, FirstName, LastName, IdentificationNumber, Email, Phone, Address, BirthDate, HireDate, BaseSalary, Status, PositionId, ScheduleId, SupervisorId, VacationDaysPerYear)
VALUES 
    (1, 'Admin', 'Sistema', '000000000', 'admin@sigep.com', '0000-0000', 'San José, Costa Rica', '1980-01-01', '2020-01-01', 1500000.00, 1, 1, 1, NULL, 14),
    (2, 'María', 'González', '101110111', 'maria.gonzalez@sigep.com', '8888-1111', 'Heredia, Costa Rica', '1985-05-15', '2021-03-15', 1200000.00, 1, 2, 1, 1, 14),
    (3, 'Juan', 'Pérez', '202220222', 'juan.perez@sigep.com', '8888-2222', 'Alajuela, Costa Rica', '1990-08-20', '2022-06-01', 600000.00, 1, 4, 1, 2, 14),
    (4, 'Ana', 'Rodríguez', '303330333', 'ana.rodriguez@sigep.com', '8888-3333', 'Cartago, Costa Rica', '1992-12-10', '2022-09-15', 700000.00, 1, 6, 1, 2, 14),
    (5, 'Carlos', 'Martínez', '404440444', 'carlos.martinez@sigep.com', '8888-4444', 'San José, Costa Rica', '1988-03-25', '2023-01-10', 450000.00, 1, 5, 1, 3, 14),
    (6, 'Laura', 'Vargas', '505550555', 'laura.vargas@sigep.com', '8888-5555', 'Heredia, Costa Rica', '1995-07-30', '2023-04-01', 650000.00, 1, 7, 1, 2, 14),
    (7, 'Roberto', 'Sánchez', '606660666', 'roberto.sanchez@sigep.com', '8888-6666', 'San José, Costa Rica', '1987-11-05', '2021-08-20', 800000.00, 1, 3, 1, 1, 14),
    (8, 'Patricia', 'Mora', '707770777', 'patricia.mora@sigep.com', '8888-7777', 'Alajuela, Costa Rica', '1993-02-14', '2024-01-15', 400000.00, 1, 8, 1, 7, 14),
    (9, 'Diego', 'Hernández', '808880888', 'diego.hernandez@sigep.com', '8888-8888', 'Cartago, Costa Rica', '1991-09-08', '2024-03-01', 350000.00, 1, 9, 2, 7, 14),
    (10, 'Sofía', 'Castro', '909990999', 'sofia.castro@sigep.com', '8888-9999', 'San José, Costa Rica', '1994-06-22', '2024-06-01', 600000.00, 1, 4, 1, 2, 14);
SET IDENTITY_INSERT Employees OFF;

-- ============================================
-- USUARIOS
-- ============================================

-- Password para todos: admin123
-- Hash BCrypt: $2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S5rKHOxYqYuASG
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, Username, Email, PasswordHash, Role, IsActive, EmployeeId)
VALUES 
    (1, 'admin', 'admin@sigep.com', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S5rKHOxYqYuASG', 1, 1, 1),
    (2, 'rrhh', 'maria.gonzalez@sigep.com', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S5rKHOxYqYuASG', 2, 1, 2),
    (3, 'juan.perez', 'juan.perez@sigep.com', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S5rKHOxYqYuASG', 4, 1, 3),
    (4, 'ana.rodriguez', 'ana.rodriguez@sigep.com', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S5rKHOxYqYuASG', 4, 1, 4),
    (5, 'carlos.martinez', 'carlos.martinez@sigep.com', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S5rKHOxYqYuASG', 4, 1, 5),
    (6, 'supervisor', 'roberto.sanchez@sigep.com', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.S5rKHOxYqYuASG', 3, 1, 7);
SET IDENTITY_INSERT Users OFF;

-- ============================================
-- SALDOS DE VACACIONES (2025 y 2026)
-- ============================================

INSERT INTO VacationBalances (EmployeeId, Year, TotalDays, UsedDays, PendingDays, CarriedOverDays, ExpirationDate)
VALUES 
    -- 2025
    (1, 2025, 14, 10, 0, 0, '2025-12-31'),
    (2, 2025, 14, 8, 0, 0, '2025-12-31'),
    (3, 2025, 14, 5, 0, 0, '2025-12-31'),
    (4, 2025, 14, 3, 0, 0, '2025-12-31'),
    (5, 2025, 14, 2, 0, 0, '2025-12-31'),
    (6, 2025, 14, 0, 0, 0, '2025-12-31'),
    (7, 2025, 14, 7, 0, 0, '2025-12-31'),
    (8, 2025, 14, 0, 0, 0, '2025-12-31'),
    (9, 2025, 14, 0, 0, 0, '2025-12-31'),
    (10, 2025, 7, 0, 0, 0, '2025-12-31'), -- Proporcional (ingresó en junio 2024)
    -- 2026
    (1, 2026, 18, 0, 0, 4, '2026-12-31'),
    (2, 2026, 20, 0, 0, 6, '2026-12-31'),
    (3, 2026, 23, 0, 5, 9, '2026-12-31'),
    (4, 2026, 25, 0, 4, 11, '2026-12-31'),
    (5, 2026, 26, 0, 0, 12, '2026-12-31'),
    (6, 2026, 28, 0, 0, 14, '2026-12-31'),
    (7, 2026, 21, 0, 0, 7, '2026-12-31'),
    (8, 2026, 28, 0, 0, 14, '2026-12-31'),
    (9, 2026, 28, 0, 0, 14, '2026-12-31'),
    (10, 2026, 21, 0, 0, 7, '2026-12-31');

-- ============================================
-- SOLICITUDES DE VACACIONES DE EJEMPLO
-- ============================================

INSERT INTO VacationRequests (EmployeeId, StartDate, EndDate, RequestedDays, Status, Reason, ApprovedByUserId, ApprovedAt, ApproverComments)
VALUES 
    -- Solicitud aprobada
    (3, '2026-03-15', '2026-03-20', 5, 2, 'Viaje familiar planificado', 2, '2026-02-01 10:30:00', 'Aprobado sin inconvenientes'),
    -- Solicitud pendiente
    (4, '2026-04-01', '2026-04-05', 4, 1, 'Vacaciones de Semana Santa', NULL, NULL, NULL),
    -- Solicitud rechazada
    (5, '2026-02-10', '2026-02-15', 5, 3, 'Necesito descanso', 2, '2026-02-03 09:00:00', 'Periodo crítico de trabajo, favor reprogramar');

-- ============================================
-- SOLICITUDES DE PERMISOS DE EJEMPLO
-- ============================================

INSERT INTO PermissionRequests (EmployeeId, PermissionTypeId, StartDate, EndDate, IsPartialDay, DurationDays, Reason, Status, ApprovedByUserId, ApprovedAt, ApproverComments)
VALUES 
    -- Permiso aprobado
    (3, 2, '2026-02-10', '2026-02-10', 0, 1, 'Cita médica programada con especialista', 2, 2, '2026-02-05 11:00:00', 'Aprobado'),
    -- Permiso pendiente
    (4, 1, '2026-02-20', '2026-02-20', 0, 1, 'Trámite personal en banco', 1, NULL, NULL, NULL),
    -- Permiso de duelo (aprobado automáticamente)
    (7, 6, '2026-01-15', '2026-01-17', 0, 3, 'Fallecimiento de familiar cercano', 2, 2, '2026-01-15 08:00:00', 'Aprobado - Permiso de duelo');

-- ============================================
-- CONFIGURACIÓN DEL SISTEMA
-- ============================================

INSERT INTO SystemSettings (SettingKey, SettingValue, Description, DataType, Category, IsEditable)
VALUES 
    ('VACATION_DAYS_PER_YEAR', '14', 'Días de vacaciones por año', 'int', 'VACATIONS', 1),
    ('VACATION_CARRYOVER_ENABLED', 'true', 'Permitir traslado de días de vacaciones', 'bool', 'VACATIONS', 1),
    ('VACATION_MAX_CARRYOVER_DAYS', '14', 'Máximo días trasladables', 'int', 'VACATIONS', 1),
    ('OVERTIME_MULTIPLIER_NORMAL', '1.5', 'Multiplicador horas extra normales', 'decimal', 'OVERTIME', 1),
    ('OVERTIME_MULTIPLIER_HOLIDAY', '2.0', 'Multiplicador horas extra feriados', 'decimal', 'OVERTIME', 1),
    ('PAYROLL_CCSS_EMPLOYEE_RATE', '0.1067', 'Porcentaje CCSS empleado', 'decimal', 'PAYROLL', 1),
    ('PAYROLL_CCSS_EMPLOYER_RATE', '0.2633', 'Porcentaje CCSS patrono', 'decimal', 'PAYROLL', 1),
    ('BONUS_CALCULATION_MONTHS', '12', 'Meses para cálculo de aguinaldo', 'int', 'BONUS', 1),
    ('BONUS_PERIOD_START_MONTH', '12', 'Mes inicio periodo aguinaldo', 'int', 'BONUS', 1),
    ('BONUS_PERIOD_START_DAY', '1', 'Día inicio periodo aguinaldo', 'int', 'BONUS', 1),
    ('COMPANY_NAME', 'Alquileres Segura', 'Nombre de la empresa', 'string', 'GENERAL', 1),
    ('COMPANY_ID', '3-101-123456', 'Cédula jurídica', 'string', 'GENERAL', 1);

PRINT 'Seed completado exitosamente';
PRINT '==============================';
PRINT 'Usuarios de prueba:';
PRINT '  admin / admin123 (Administrador)';
PRINT '  rrhh / admin123 (RRHH)';
PRINT '  supervisor / admin123 (Jefatura)';
PRINT '  juan.perez / admin123 (Empleado)';
PRINT '  ana.rodriguez / admin123 (Empleado)';
PRINT '  carlos.martinez / admin123 (Empleado)';
PRINT '==============================';
