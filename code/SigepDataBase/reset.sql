-- ============================================
-- SIGEP - Reset Completo de Base de Datos
-- SQL Server
-- Ejecuta: DROP + CREATE DB + Schema + Seed
-- ============================================

USE master;
GO

-- Cerrar conexiones existentes y eliminar la base de datos si existe
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'SigepDB')
BEGIN
    ALTER DATABASE SigepDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SigepDB;
END
GO

-- Crear la base de datos
CREATE DATABASE SigepDB;
GO

USE SigepDB;
GO

-- ============================================
-- SCHEMA
-- ============================================

-- Horarios de trabajo
CREATE TABLE Schedules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    WorkHoursPerDay INT NOT NULL DEFAULT 8,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Posiciones/Cargos
CREATE TABLE Positions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    BaseSalary DECIMAL(18,2) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Tipos de Permisos
CREATE TABLE PermissionTypes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    MaxDaysPerYear INT NULL,
    RequiresApproval BIT NOT NULL DEFAULT 1,
    IsPaid BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Tipos de Deducciones
CREATE TABLE DeductionTypes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsPercentage BIT NOT NULL DEFAULT 0,
    DefaultValue DECIMAL(18,4) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Tipos de Beneficios
CREATE TABLE BenefitTypes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsPercentage BIT NOT NULL DEFAULT 0,
    DefaultValue DECIMAL(18,4) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Empleados
CREATE TABLE Employees (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    IdentificationNumber NVARCHAR(50) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Address NVARCHAR(500) NULL,
    BirthDate DATE NULL,
    HireDate DATE NOT NULL,
    TerminationDate DATE NULL,
    BaseSalary DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    PositionId INT NULL,
    ScheduleId INT NULL,
    SupervisorId INT NULL,
    VacationDaysPerYear INT NOT NULL DEFAULT 14,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Employees_Positions FOREIGN KEY (PositionId) REFERENCES Positions(Id),
    CONSTRAINT FK_Employees_Schedules FOREIGN KEY (ScheduleId) REFERENCES Schedules(Id),
    CONSTRAINT FK_Employees_Supervisor FOREIGN KEY (SupervisorId) REFERENCES Employees(Id),
    CONSTRAINT UQ_Employees_IdentificationNumber UNIQUE (IdentificationNumber),
    CONSTRAINT UQ_Employees_Email UNIQUE (Email)
);

-- Usuarios
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role INT NOT NULL DEFAULT 4,
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    EmployeeId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Users_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

-- Bitácora de Auditoría
CREATE TABLE AuditLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    Module NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100) NULL,
    EntityId INT NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    Description NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Notificaciones
CREATE TABLE Notifications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Module NVARCHAR(100) NULL,
    EntityType NVARCHAR(100) NULL,
    EntityId INT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Saldo de Vacaciones
CREATE TABLE VacationBalances (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Year INT NOT NULL,
    TotalDays INT NOT NULL DEFAULT 0,
    UsedDays INT NOT NULL DEFAULT 0,
    PendingDays INT NOT NULL DEFAULT 0,
    AvailableDays AS (TotalDays - UsedDays - PendingDays) PERSISTED,
    CarryOverDays INT NOT NULL DEFAULT 0,
    ExpirationDate DATE NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_VacationBalances_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_VacationBalances_Employee_Year UNIQUE (EmployeeId, Year)
);

-- Solicitudes de Vacaciones
CREATE TABLE VacationRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    TotalDays INT NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    Comments NVARCHAR(1000) NULL,
    ReviewedById INT NULL,
    ReviewedAt DATETIME2 NULL,
    ReviewComments NVARCHAR(1000) NULL,
    Version INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_VacationRequests_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_VacationRequests_ReviewedBy FOREIGN KEY (ReviewedById) REFERENCES Users(Id),
    CONSTRAINT CK_VacationRequests_Dates CHECK (EndDate >= StartDate)
);

-- Historial de Vacaciones
CREATE TABLE VacationRequestHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VacationRequestId INT NOT NULL,
    Status INT NOT NULL,
    ChangedById INT NOT NULL,
    Comments NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_VacationRequestHistory_Request FOREIGN KEY (VacationRequestId) REFERENCES VacationRequests(Id),
    CONSTRAINT FK_VacationRequestHistory_ChangedBy FOREIGN KEY (ChangedById) REFERENCES Users(Id)
);

-- Solicitudes de Permisos
CREATE TABLE PermissionRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    PermissionTypeId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    StartTime TIME NULL,
    EndTime TIME NULL,
    TotalDays DECIMAL(5,2) NOT NULL,
    Reason NVARCHAR(1000) NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    AttachmentPath NVARCHAR(500) NULL,
    ReviewedById INT NULL,
    ReviewedAt DATETIME2 NULL,
    ReviewComments NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_PermissionRequests_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_PermissionRequests_PermissionTypes FOREIGN KEY (PermissionTypeId) REFERENCES PermissionTypes(Id),
    CONSTRAINT FK_PermissionRequests_ReviewedBy FOREIGN KEY (ReviewedById) REFERENCES Users(Id),
    CONSTRAINT CK_PermissionRequests_Dates CHECK (EndDate >= StartDate)
);

-- Registros de Asistencia
CREATE TABLE AttendanceRecords (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Date DATE NOT NULL,
    CheckInTime DATETIME2 NULL,
    CheckOutTime DATETIME2 NULL,
    CheckInLatitude DECIMAL(10,7) NULL,
    CheckInLongitude DECIMAL(10,7) NULL,
    CheckOutLatitude DECIMAL(10,7) NULL,
    CheckOutLongitude DECIMAL(10,7) NULL,
    WorkedHours DECIMAL(5,2) NULL,
    Status INT NOT NULL DEFAULT 1,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AttendanceRecords_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_AttendanceRecords_Employee_Date UNIQUE (EmployeeId, Date)
);

-- Horas Extra
CREATE TABLE OvertimeRecords (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    AttendanceId INT NULL,
    Date DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    TotalHours DECIMAL(5,2) NOT NULL,
    HourlyRate DECIMAL(18,2) NOT NULL,
    MultiplierRate DECIMAL(3,2) NOT NULL DEFAULT 1.5,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    DetectionType INT NOT NULL DEFAULT 1,
    ReviewedById INT NULL,
    ReviewedAt DATETIME2 NULL,
    ReviewComments NVARCHAR(1000) NULL,
    PayrollDetailId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_OvertimeRecords_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_OvertimeRecords_Attendance FOREIGN KEY (AttendanceId) REFERENCES AttendanceRecords(Id),
    CONSTRAINT FK_OvertimeRecords_ReviewedBy FOREIGN KEY (ReviewedById) REFERENCES Users(Id)
);

-- Planillas (cabecera)
CREATE TABLE Payrolls (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PeriodYear INT NOT NULL,
    PeriodMonth INT NOT NULL,
    PeriodType INT NOT NULL,
    PeriodStartDate DATE NOT NULL,
    PeriodEndDate DATE NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    TotalGrossSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalDeductions DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalBenefits DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalNetSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalEmployees INT NOT NULL DEFAULT 0,
    ProcessedById INT NULL,
    ProcessedAt DATETIME2 NULL,
    ApprovedById INT NULL,
    ApprovedAt DATETIME2 NULL,
    Notes NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Payrolls_ProcessedBy FOREIGN KEY (ProcessedById) REFERENCES Users(Id),
    CONSTRAINT FK_Payrolls_ApprovedBy FOREIGN KEY (ApprovedById) REFERENCES Users(Id),
    CONSTRAINT UQ_Payrolls_Period UNIQUE (PeriodYear, PeriodMonth, PeriodType)
);

-- Detalle de Planilla
CREATE TABLE PayrollDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PayrollId INT NOT NULL,
    EmployeeId INT NOT NULL,
    BaseSalary DECIMAL(18,2) NOT NULL,
    WorkedDays INT NOT NULL DEFAULT 0,
    OvertimeHours DECIMAL(5,2) NOT NULL DEFAULT 0,
    OvertimeAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    GrossSalary DECIMAL(18,2) NOT NULL,
    TotalDeductions DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalBenefits DECIMAL(18,2) NOT NULL DEFAULT 0,
    NetSalary DECIMAL(18,2) NOT NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_PayrollDetails_Payrolls FOREIGN KEY (PayrollId) REFERENCES Payrolls(Id),
    CONSTRAINT FK_PayrollDetails_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_PayrollDetails_Payroll_Employee UNIQUE (PayrollId, EmployeeId)
);

-- Deducciones de Planilla
CREATE TABLE PayrollDeductions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PayrollDetailId INT NOT NULL,
    DeductionTypeId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    IsPercentage BIT NOT NULL DEFAULT 0,
    PercentageValue DECIMAL(5,4) NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_PayrollDeductions_PayrollDetails FOREIGN KEY (PayrollDetailId) REFERENCES PayrollDetails(Id),
    CONSTRAINT FK_PayrollDeductions_DeductionTypes FOREIGN KEY (DeductionTypeId) REFERENCES DeductionTypes(Id)
);

-- Beneficios de Planilla
CREATE TABLE PayrollBenefits (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PayrollDetailId INT NOT NULL,
    BenefitTypeId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    IsPercentage BIT NOT NULL DEFAULT 0,
    PercentageValue DECIMAL(5,4) NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_PayrollBenefits_PayrollDetails FOREIGN KEY (PayrollDetailId) REFERENCES PayrollDetails(Id),
    CONSTRAINT FK_PayrollBenefits_BenefitTypes FOREIGN KEY (BenefitTypeId) REFERENCES BenefitTypes(Id)
);

-- Liquidaciones
CREATE TABLE Settlements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    TerminationType INT NOT NULL,
    HireDate DATE NOT NULL,
    TerminationDate DATE NOT NULL,
    LastSalary DECIMAL(18,2) NOT NULL,
    AverageSalary DECIMAL(18,2) NOT NULL,
    WorkedYears INT NOT NULL,
    WorkedMonths INT NOT NULL,
    WorkedDays INT NOT NULL,
    PendingVacationDays DECIMAL(5,2) NOT NULL DEFAULT 0,
    VacationAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ProportionalBonus DECIMAL(18,2) NOT NULL DEFAULT 0,
    SeveranceAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    OtherBenefits DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalDeductions DECIMAL(18,2) NOT NULL DEFAULT 0,
    GrossTotal DECIMAL(18,2) NOT NULL,
    NetTotal DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    CalculatedById INT NOT NULL,
    CalculatedAt DATETIME2 NOT NULL,
    ApprovedById INT NULL,
    ApprovedAt DATETIME2 NULL,
    Notes NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Settlements_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_Settlements_CalculatedBy FOREIGN KEY (CalculatedById) REFERENCES Users(Id),
    CONSTRAINT FK_Settlements_ApprovedBy FOREIGN KEY (ApprovedById) REFERENCES Users(Id)
);

-- Deducciones de Liquidación
CREATE TABLE SettlementDeductions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SettlementId INT NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_SettlementDeductions_Settlements FOREIGN KEY (SettlementId) REFERENCES Settlements(Id)
);

-- Aguinaldos (cabecera)
CREATE TABLE AnnualBonuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Year INT NOT NULL,
    PeriodStartDate DATE NOT NULL,
    PeriodEndDate DATE NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalEmployees INT NOT NULL DEFAULT 0,
    CalculatedById INT NOT NULL,
    CalculatedAt DATETIME2 NOT NULL,
    ApprovedById INT NULL,
    ApprovedAt DATETIME2 NULL,
    Notes NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_AnnualBonuses_CalculatedBy FOREIGN KEY (CalculatedById) REFERENCES Users(Id),
    CONSTRAINT FK_AnnualBonuses_ApprovedBy FOREIGN KEY (ApprovedById) REFERENCES Users(Id),
    CONSTRAINT UQ_AnnualBonuses_Year UNIQUE (Year)
);

-- Detalle de Aguinaldo
CREATE TABLE AnnualBonusDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AnnualBonusId INT NOT NULL,
    EmployeeId INT NOT NULL,
    WorkedMonths INT NOT NULL,
    AverageSalary DECIMAL(18,2) NOT NULL,
    ProportionalAmount DECIMAL(18,2) NOT NULL,
    Deductions DECIMAL(18,2) NOT NULL DEFAULT 0,
    NetAmount DECIMAL(18,2) NOT NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AnnualBonusDetails_AnnualBonuses FOREIGN KEY (AnnualBonusId) REFERENCES AnnualBonuses(Id),
    CONSTRAINT FK_AnnualBonusDetails_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_AnnualBonusDetails_Bonus_Employee UNIQUE (AnnualBonusId, EmployeeId)
);

-- Evaluaciones de Desempeño
CREATE TABLE PerformanceEvaluations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    EvaluatorId INT NOT NULL,
    EvaluationDate DATE NOT NULL,
    PeriodStartDate DATE NULL,
    PeriodEndDate DATE NULL,
    Score INT NOT NULL,
    Comments NVARCHAR(2000) NULL,
    Strengths NVARCHAR(1000) NULL,
    AreasToImprove NVARCHAR(1000) NULL,
    Goals NVARCHAR(1000) NULL,
    Status INT NOT NULL DEFAULT 1,
    EmployeeAcknowledged BIT NOT NULL DEFAULT 0,
    AcknowledgedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_PerformanceEvaluations_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_PerformanceEvaluations_Evaluator FOREIGN KEY (EvaluatorId) REFERENCES Users(Id),
    CONSTRAINT CK_PerformanceEvaluations_Score CHECK (Score >= 3 AND Score <= 10)
);

-- Incapacidades
CREATE TABLE DisabilityRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    TotalDays INT NOT NULL,
    Type INT NOT NULL,
    Diagnosis NVARCHAR(500) NULL,
    DoctorName NVARCHAR(200) NULL,
    MedicalCenter NVARCHAR(200) NULL,
    DocumentNumber NVARCHAR(100) NULL,
    AttachmentPath NVARCHAR(500) NULL,
    Status INT NOT NULL DEFAULT 1,
    ReviewedById INT NULL,
    ReviewedAt DATETIME2 NULL,
    ReviewComments NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_DisabilityRequests_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_DisabilityRequests_ReviewedBy FOREIGN KEY (ReviewedById) REFERENCES Users(Id),
    CONSTRAINT CK_DisabilityRequests_Dates CHECK (EndDate >= StartDate)
);

-- Configuración del Sistema
CREATE TABLE SystemSettings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(500) NULL,
    DataType NVARCHAR(50) NOT NULL DEFAULT 'string',
    Category NVARCHAR(100) NULL,
    IsEditable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT UQ_SystemSettings_Key UNIQUE (SettingKey)
);

-- ============================================
-- ÍNDICES
-- ============================================

CREATE INDEX IX_Employees_PositionId ON Employees(PositionId);
CREATE INDEX IX_Employees_ScheduleId ON Employees(ScheduleId);
CREATE INDEX IX_Employees_Status ON Employees(Status);
CREATE INDEX IX_Employees_HireDate ON Employees(HireDate);
CREATE INDEX IX_Users_EmployeeId ON Users(EmployeeId);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Module ON AuditLogs(Module);
CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);
CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_VacationRequests_EmployeeId ON VacationRequests(EmployeeId);
CREATE INDEX IX_VacationRequests_Status ON VacationRequests(Status);
CREATE INDEX IX_VacationRequests_StartDate ON VacationRequests(StartDate);
CREATE INDEX IX_PermissionRequests_EmployeeId ON PermissionRequests(EmployeeId);
CREATE INDEX IX_PermissionRequests_Status ON PermissionRequests(Status);
CREATE INDEX IX_AttendanceRecords_Date ON AttendanceRecords(Date);
CREATE INDEX IX_AttendanceRecords_Status ON AttendanceRecords(Status);
CREATE INDEX IX_OvertimeRecords_EmployeeId ON OvertimeRecords(EmployeeId);
CREATE INDEX IX_OvertimeRecords_Status ON OvertimeRecords(Status);
CREATE INDEX IX_OvertimeRecords_Date ON OvertimeRecords(Date);
CREATE INDEX IX_Payrolls_Status ON Payrolls(Status);
CREATE INDEX IX_Payrolls_Period ON Payrolls(PeriodYear, PeriodMonth);
CREATE INDEX IX_PayrollDetails_EmployeeId ON PayrollDetails(EmployeeId);
CREATE INDEX IX_PerformanceEvaluations_EmployeeId ON PerformanceEvaluations(EmployeeId);
CREATE INDEX IX_PerformanceEvaluations_EvaluationDate ON PerformanceEvaluations(EvaluationDate);
CREATE INDEX IX_DisabilityRequests_EmployeeId ON DisabilityRequests(EmployeeId);
CREATE INDEX IX_DisabilityRequests_Status ON DisabilityRequests(Status);

-- ============================================
-- SEED DATA
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

-- Posiciones
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
INSERT INTO PermissionTypes (Id, Name, Description, MaxDaysPerYear, RequiresApproval, IsPaid, IsActive)
VALUES 
    (1, 'Personal', 'Permiso por asuntos personales', 5, 1, 1, 1),
    (2, 'Médico', 'Cita médica o procedimiento', 12, 1, 1, 1),
    (3, 'Familiar', 'Asuntos familiares urgentes', 3, 1, 1, 1),
    (4, 'Estudio', 'Permisos para estudios o exámenes', 10, 1, 0, 1),
    (5, 'Matrimonio', 'Permiso por matrimonio', 3, 1, 1, 1),
    (6, 'Duelo', 'Fallecimiento de familiar', 5, 0, 1, 1),
    (7, 'Nacimiento', 'Nacimiento de hijo', 3, 0, 1, 1),
    (8, 'Judicial', 'Citación judicial o legal', NULL, 1, 1, 1),
    (9, 'Otro', 'Otros permisos', NULL, 1, 0, 1);
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

-- Empleados
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

-- Usuarios (Password: admin123)
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

-- Saldos de Vacaciones
INSERT INTO VacationBalances (EmployeeId, Year, TotalDays, UsedDays, PendingDays, CarryOverDays)
VALUES 
    (1, 2025, 14, 10, 0, 0), (2, 2025, 14, 8, 0, 0), (3, 2025, 14, 5, 0, 0),
    (4, 2025, 14, 3, 0, 0), (5, 2025, 14, 2, 0, 0), (6, 2025, 14, 0, 0, 0),
    (7, 2025, 14, 7, 0, 0), (8, 2025, 14, 0, 0, 0), (9, 2025, 14, 0, 0, 0),
    (10, 2025, 7, 0, 0, 0),
    (1, 2026, 14, 0, 0, 4), (2, 2026, 14, 0, 0, 6), (3, 2026, 14, 0, 0, 9),
    (4, 2026, 14, 0, 0, 11), (5, 2026, 14, 0, 0, 12), (6, 2026, 14, 0, 0, 14),
    (7, 2026, 14, 0, 0, 7), (8, 2026, 14, 0, 0, 14), (9, 2026, 14, 0, 0, 14),
    (10, 2026, 14, 0, 0, 7);

-- Solicitudes de Vacaciones de ejemplo
INSERT INTO VacationRequests (EmployeeId, StartDate, EndDate, TotalDays, Status, Comments, ReviewedById, ReviewedAt, ReviewComments, Version)
VALUES 
    (3, '2026-03-15', '2026-03-20', 5, 2, 'Viaje familiar planificado', 2, '2026-02-01 10:30:00', 'Aprobado sin inconvenientes', 1),
    (4, '2026-04-01', '2026-04-05', 4, 1, 'Vacaciones de Semana Santa', NULL, NULL, NULL, 1),
    (5, '2026-02-10', '2026-02-15', 5, 3, 'Necesito descanso', 2, '2026-02-03 09:00:00', 'Periodo crítico de trabajo, favor reprogramar', 1);

-- Solicitudes de Permisos de ejemplo
INSERT INTO PermissionRequests (EmployeeId, PermissionTypeId, StartDate, EndDate, TotalDays, Reason, Status, ReviewedById, ReviewedAt, ReviewComments)
VALUES 
    (3, 2, '2026-02-10', '2026-02-10', 1, 'Cita médica programada con especialista', 2, 2, '2026-02-05 11:00:00', 'Aprobado'),
    (4, 1, '2026-02-20', '2026-02-20', 1, 'Trámite personal en banco', 1, NULL, NULL, NULL),
    (7, 6, '2026-01-15', '2026-01-17', 3, 'Fallecimiento de familiar cercano', 2, 2, '2026-01-15 08:00:00', 'Aprobado - Permiso de duelo');

-- Configuración del Sistema
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

PRINT '========================================';
PRINT 'Base de datos SigepDB creada exitosamente';
PRINT '========================================';
PRINT '';
PRINT 'Usuarios de prueba (password: admin123):';
PRINT '  - admin (Administrador)';
PRINT '  - rrhh (RRHH)';
PRINT '  - supervisor (Jefatura)';
PRINT '  - juan.perez (Empleado)';
PRINT '  - ana.rodriguez (Empleado)';
PRINT '  - carlos.martinez (Empleado)';
PRINT '';
PRINT '========================================';
GO
