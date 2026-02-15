-- ============================================
-- SIGEP - Schema Completo de Base de Datos
-- SQL Server
-- Versión: 1.0
-- ============================================

-- ============================================
-- TABLAS BASE / CATÁLOGOS
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

-- Tipos de Permisos (catálogo)
CREATE TABLE PermissionTypes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    MaxDaysPerYear INT NULL,
    RequiresDocument BIT NOT NULL DEFAULT 0,
    IsPaid BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Tipos de Deducciones (catálogo)
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

-- Tipos de Beneficios (catálogo)
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

-- ============================================
-- EMPLEADOS Y USUARIOS
-- ============================================

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
    Status INT NOT NULL DEFAULT 1, -- 1=Activo, 2=Inactivo, 3=Suspendido, 4=Liquidado
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

-- Usuarios del sistema
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role INT NOT NULL DEFAULT 4, -- 1=Admin, 2=RRHH, 3=Jefatura, 4=Empleado
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    EmployeeId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Users_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

-- ============================================
-- MÓDULO: BITÁCORA DE AUDITORÍA
-- ============================================

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

-- ============================================
-- MÓDULO: NOTIFICACIONES
-- ============================================

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

-- ============================================
-- MÓDULO: VACACIONES
-- ============================================

-- Saldo de vacaciones por empleado por año
CREATE TABLE VacationBalances (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    Year INT NOT NULL,
    TotalDays DECIMAL(5,2) NOT NULL DEFAULT 0,
    UsedDays DECIMAL(5,2) NOT NULL DEFAULT 0,
    PendingDays DECIMAL(5,2) NOT NULL DEFAULT 0,
    AvailableDays AS (TotalDays - UsedDays - PendingDays) PERSISTED,
    CarriedOverDays DECIMAL(5,2) NOT NULL DEFAULT 0,
    ExpirationDate DATE NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_VacationBalances_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_VacationBalances_Employee_Year UNIQUE (EmployeeId, Year)
);

-- Solicitudes de vacaciones
CREATE TABLE VacationRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    RequestedDays INT NOT NULL,
    Reason NVARCHAR(1000) NULL,
    Status INT NOT NULL DEFAULT 1, -- 1=Pendiente, 2=Aprobada, 3=Rechazada, 4=Cancelada, 5=EnRevision
    ApprovedByUserId INT NULL,
    ApprovedAt DATETIME2 NULL,
    ApproverComments NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_VacationRequests_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_VacationRequests_ApprovedBy FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id),
    CONSTRAINT CK_VacationRequests_Dates CHECK (EndDate >= StartDate)
);

-- Historial de cambios en solicitudes de vacaciones
CREATE TABLE VacationRequestHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VacationRequestId INT NOT NULL,
    Status INT NOT NULL,
    Comments NVARCHAR(1000) NULL,
    ChangedByUserId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_VacationRequestHistory_Request FOREIGN KEY (VacationRequestId) REFERENCES VacationRequests(Id),
    CONSTRAINT FK_VacationRequestHistory_ChangedBy FOREIGN KEY (ChangedByUserId) REFERENCES Users(Id)
);

-- ============================================
-- MÓDULO: PERMISOS
-- ============================================

-- Solicitudes de permisos
CREATE TABLE PermissionRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    PermissionTypeId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    StartTime TIME NULL,
    EndTime TIME NULL,
    IsPartialDay BIT NOT NULL DEFAULT 0,
    DurationDays DECIMAL(5,2) NOT NULL,
    Reason NVARCHAR(1000) NOT NULL,
    DocumentUrl NVARCHAR(500) NULL,
    Status INT NOT NULL DEFAULT 1, -- 1=Pendiente, 2=Aprobada, 3=Rechazada, 4=Cancelada
    ApprovedByUserId INT NULL,
    ApprovedAt DATETIME2 NULL,
    ApproverComments NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_PermissionRequests_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_PermissionRequests_PermissionTypes FOREIGN KEY (PermissionTypeId) REFERENCES PermissionTypes(Id),
    CONSTRAINT FK_PermissionRequests_ApprovedBy FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id),
    CONSTRAINT CK_PermissionRequests_Dates CHECK (EndDate >= StartDate)
);

-- ============================================
-- MÓDULO: ASISTENCIA
-- ============================================

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
    Status INT NOT NULL DEFAULT 1, -- 1=Parcial, 2=Completo, 3=Ausente, 4=Permiso, 5=Vacaciones, 6=Incapacidad
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AttendanceRecords_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT UQ_AttendanceRecords_Employee_Date UNIQUE (EmployeeId, Date)
);

-- ============================================
-- MÓDULO: HORAS EXTRA
-- ============================================

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
    Status INT NOT NULL DEFAULT 1, -- 1=Detectada, 2=Pendiente, 3=Aprobada, 4=Rechazada, 5=Pagada
    DetectionType INT NOT NULL DEFAULT 1, -- 1=Automática, 2=Manual
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

-- ============================================
-- MÓDULO: PLANILLA
-- ============================================

-- Planillas (cabecera)
CREATE TABLE Payrolls (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PeriodYear INT NOT NULL,
    PeriodMonth INT NOT NULL,
    PeriodType INT NOT NULL, -- 1=Primera quincena, 2=Segunda quincena, 3=Mensual
    PeriodStartDate DATE NOT NULL,
    PeriodEndDate DATE NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- 1=Borrador, 2=Procesando, 3=Completada, 4=Anulada
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

-- Detalle de planilla por empleado
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

-- Deducciones aplicadas en planilla
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

-- Beneficios aplicados en planilla
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

-- ============================================
-- MÓDULO: LIQUIDACIONES
-- ============================================

CREATE TABLE Settlements (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    TerminationType INT NOT NULL, -- 1=Renuncia, 2=Despido con responsabilidad, 3=Despido sin responsabilidad, 4=Mutuo acuerdo, 5=Jubilación
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
    Status INT NOT NULL DEFAULT 1, -- 1=Borrador, 2=Calculada, 3=Aprobada, 4=Pagada, 5=Anulada
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

-- Detalle de deducciones en liquidación
CREATE TABLE SettlementDeductions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SettlementId INT NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_SettlementDeductions_Settlements FOREIGN KEY (SettlementId) REFERENCES Settlements(Id)
);

-- ============================================
-- MÓDULO: AGUINALDO
-- ============================================

-- Cálculo de aguinaldo anual (cabecera)
CREATE TABLE AnnualBonuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Year INT NOT NULL,
    PeriodStartDate DATE NOT NULL,
    PeriodEndDate DATE NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- 1=Borrador, 2=Calculado, 3=Aprobado, 4=Pagado, 5=Anulado
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

-- Detalle de aguinaldo por empleado
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

-- ============================================
-- MÓDULO: EVALUACIÓN DE DESEMPEÑO
-- ============================================

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
    Status INT NOT NULL DEFAULT 1, -- 1=Borrador, 2=Completada, 3=Revisada por empleado
    EmployeeAcknowledged BIT NOT NULL DEFAULT 0,
    AcknowledgedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_PerformanceEvaluations_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_PerformanceEvaluations_Evaluator FOREIGN KEY (EvaluatorId) REFERENCES Users(Id),
    CONSTRAINT CK_PerformanceEvaluations_Score CHECK (Score >= 3 AND Score <= 10)
);

-- ============================================
-- MÓDULO: INCAPACIDADES
-- ============================================

CREATE TABLE DisabilityRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    TotalDays INT NOT NULL,
    Type INT NOT NULL, -- 1=Enfermedad común, 2=Accidente laboral, 3=Maternidad, 4=Otro
    Diagnosis NVARCHAR(500) NULL,
    DoctorName NVARCHAR(200) NULL,
    MedicalCenter NVARCHAR(200) NULL,
    DocumentNumber NVARCHAR(100) NULL,
    AttachmentPath NVARCHAR(500) NULL,
    Status INT NOT NULL DEFAULT 1, -- 1=Pendiente, 2=Aprobada, 3=Rechazada
    ReviewedById INT NULL,
    ReviewedAt DATETIME2 NULL,
    ReviewComments NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_DisabilityRequests_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    CONSTRAINT FK_DisabilityRequests_ReviewedBy FOREIGN KEY (ReviewedById) REFERENCES Users(Id),
    CONSTRAINT CK_DisabilityRequests_Dates CHECK (EndDate >= StartDate)
);

-- ============================================
-- MÓDULO: CONFIGURACIÓN DEL SISTEMA
-- ============================================

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
-- ÍNDICES ADICIONALES
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
