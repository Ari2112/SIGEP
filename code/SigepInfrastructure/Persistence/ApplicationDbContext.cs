using Microsoft.EntityFrameworkCore;
using SigepDomain.Entities;

namespace SigepInfrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // =========================
    // ENTIDADES BASE
    // =========================
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeStatus> EmployeeStatuses { get; set; }

    public DbSet<Position> Positions { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<ScheduleDay> ScheduleDays { get; set; }

    // Teléfonos de empleados
    public DbSet<EmployeePhone> EmployeePhones { get; set; }
    public DbSet<PhoneType> PhoneTypes { get; set; }

    // Direcciones de empleados
    public DbSet<EmployeeAddress> EmployeeAddresses { get; set; }
    public DbSet<AddressType> AddressTypes { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<Canton> Cantons { get; set; }
    public DbSet<District> Districts { get; set; }

    // Feriados
    public DbSet<PublicHoliday> PublicHolidays { get; set; }

    // Módulo transversal
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    // Vacaciones
    public DbSet<VacationBalance> VacationBalances { get; set; }
    public DbSet<VacationRequest> VacationRequests { get; set; }
    public DbSet<VacationRequestHistory> VacationRequestHistory { get; set; }

    // Permisos
    public DbSet<PermissionType> PermissionTypes { get; set; }
    public DbSet<PermissionRequest> PermissionRequests { get; set; }

    // Estados generales de solicitudes
    public DbSet<RequestStatus> RequestStatuses { get; set; }

    // Asistencia
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<AttendanceStatus> AttendanceStatuses { get; set; }

    // Horas extra
    public DbSet<OvertimeRecord> OvertimeRecords { get; set; }

    // Planilla
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<PayrollDetail> PayrollDetails { get; set; }
    public DbSet<PayrollDeduction> PayrollDeductions { get; set; }
    public DbSet<PayrollBenefit> PayrollBenefits { get; set; }
    public DbSet<DeductionType> DeductionTypes { get; set; }
    public DbSet<BenefitType> BenefitTypes { get; set; }
    public DbSet<PayrollStatus> PayrollStatuses { get; set; }
    public DbSet<PayrollPeriodType> PayrollPeriodTypes { get; set; }

    // Liquidaciones
    public DbSet<Settlement> Settlements { get; set; }
    public DbSet<SettlementDeduction> SettlementDeductions { get; set; }
    public DbSet<TerminationType> TerminationTypes { get; set; }

    // Aguinaldo
    public DbSet<AnnualBonus> AnnualBonuses { get; set; }
    public DbSet<AnnualBonusDetail> AnnualBonusDetails { get; set; }

    // Evaluación de desempeño
    public DbSet<PerformanceEvaluation> PerformanceEvaluations { get; set; }

    // Incapacidades
    public DbSet<DisabilityRequest> DisabilityRequests { get; set; }
    public DbSet<DisabilityType> DisabilityTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // EMPLOYEES
        // =========================
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.IdentificationNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.BaseSalary)
                .HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.IdentificationNumber)
                .IsUnique();

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.HasOne(e => e.EmployeeStatus)
                .WithMany(es => es.Employees)
                .HasForeignKey(e => e.EmployeeStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Schedule)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.ScheduleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Supervisor)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.FullName);
        });

        modelBuilder.Entity<EmployeeStatus>(entity =>
        {
            entity.ToTable("EmployeeStatuses");

            entity.HasKey(es => es.Id);

            entity.Property(es => es.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(es => es.Description)
                .HasMaxLength(500);

            entity.HasIndex(es => es.Name)
                .IsUnique();
        });

        // =========================
        // USERS / ROLES
        // =========================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(u => u.IsActive)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .IsRequired();

            entity.HasIndex(u => u.Username)
                .IsUnique();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Employee)
                .WithOne(e => e.User)
                .HasForeignKey<User>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(r => r.Description)
                .HasMaxLength(500);

            entity.HasIndex(r => r.Name)
                .IsUnique();
        });

        // =========================
        // EMPLOYEE PHONES
        // =========================
        modelBuilder.Entity<EmployeePhone>(entity =>
        {
            entity.ToTable("EmployeePhones");

            entity.HasKey(ep => ep.Id);

            entity.Property(ep => ep.PhoneNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(ep => ep.Employee)
                .WithMany(e => e.EmployeePhones)
                .HasForeignKey(ep => ep.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ep => ep.PhoneType)
                .WithMany(pt => pt.EmployeePhones)
                .HasForeignKey(ep => ep.PhoneTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PhoneType>(entity =>
        {
            entity.ToTable("PhoneTypes");

            entity.HasKey(pt => pt.Id);

            entity.Property(pt => pt.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(pt => pt.Description)
                .HasMaxLength(500);

            entity.HasIndex(pt => pt.Name)
                .IsUnique();
        });

        // =========================
        // EMPLOYEE ADDRESSES
        // =========================
        modelBuilder.Entity<EmployeeAddress>(entity =>
        {
            entity.ToTable("EmployeeAddresses");

            entity.HasKey(ea => ea.Id);

            entity.Property(ea => ea.ExactAddress)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasOne(ea => ea.Employee)
                .WithMany(e => e.EmployeeAddresses)
                .HasForeignKey(ea => ea.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ea => ea.AddressType)
                .WithMany(at => at.EmployeeAddresses)
                .HasForeignKey(ea => ea.AddressTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ea => ea.Province)
                .WithMany(p => p.EmployeeAddresses)
                .HasForeignKey(ea => ea.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ea => ea.Canton)
                .WithMany(c => c.EmployeeAddresses)
                .HasForeignKey(ea => ea.CantonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ea => ea.District)
                .WithMany(d => d.EmployeeAddresses)
                .HasForeignKey(ea => ea.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AddressType>(entity =>
        {
            entity.ToTable("AddressTypes");

            entity.HasKey(at => at.Id);

            entity.Property(at => at.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(at => at.Description)
                .HasMaxLength(500);

            entity.HasIndex(at => at.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.ToTable("Provinces");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(p => p.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Canton>(entity =>
        {
            entity.ToTable("Cantons");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(c => c.Province)
                .WithMany(p => p.Cantons)
                .HasForeignKey(c => c.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("Districts");

            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(d => d.Canton)
                .WithMany(c => c.Districts)
                .HasForeignKey(d => d.CantonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // PUBLIC HOLIDAYS
        // =========================
        modelBuilder.Entity<PublicHoliday>(entity =>
        {
            entity.ToTable("PublicHolidays");

            entity.HasKey(ph => ph.Id);

            entity.Property(ph => ph.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(ph => ph.Description)
                .HasMaxLength(500);

            entity.Property(ph => ph.Date)
                .HasColumnName("HolidayDate");

            entity.HasIndex(ph => ph.Date)
                .IsUnique();
        });

        // =========================
        // POSITIONS / SCHEDULES
        // =========================
        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("Positions");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasMaxLength(500);

            entity.Property(p => p.BaseSalary)
                .HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("Schedules");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired();
        });

        // =========================
        // VACATION BALANCES
        // =========================
        modelBuilder.Entity<VacationBalance>(entity =>
        {
            entity.ToTable("VacationBalances");

            entity.HasKey(v => v.Id);

            entity.Property(v => v.AvailableDays)
                .HasComputedColumnSql("[TotalDays] - [UsedDays] - [PendingDays]", stored: true);

            entity.Property(v => v.CarriedOverDays)
                .HasColumnName("CarryOverDays");

            entity.HasIndex(v => new { v.EmployeeId, v.Year })
                .IsUnique();

            entity.HasOne(v => v.Employee)
                .WithMany(e => e.VacationBalances)
                .HasForeignKey(v => v.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // VACATION REQUESTS
        // =========================
        modelBuilder.Entity<VacationRequest>(entity =>
        {
            entity.ToTable("VacationRequests");

            entity.HasKey(v => v.Id);

            entity.Property(v => v.RequestedDays)
                .HasColumnName("TotalDays");

            entity.Property(v => v.Reason)
                .HasColumnName("Comments");

            entity.Property(v => v.ApprovedByUserId)
                .HasColumnName("ReviewedById");

            entity.Property(v => v.ApprovedAt)
                .HasColumnName("ReviewedAt");

            entity.Property(v => v.ApproverComments)
                .HasColumnName("ReviewComments");

            entity.HasOne(v => v.Employee)
                .WithMany(e => e.VacationRequests)
                .HasForeignKey(v => v.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.ApprovedByUser)
                .WithMany()
                .HasForeignKey(v => v.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.RequestStatus)
                .WithMany(rs => rs.VacationRequests)
                .HasForeignKey(v => v.RequestStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VacationRequestHistory>(entity =>
        {
            entity.ToTable("VacationRequestHistory");

            entity.HasKey(h => h.Id);

            entity.Property(h => h.ChangedByUserId)
                .HasColumnName("ChangedById");

            entity.HasOne(h => h.VacationRequest)
                .WithMany(v => v.History)
                .HasForeignKey(h => h.VacationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(h => h.RequestStatus)
                .WithMany(rs => rs.VacationRequestHistories)
                .HasForeignKey(h => h.RequestStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // PERMISSIONS
        // =========================
        modelBuilder.Entity<PermissionType>(entity =>
        {
            entity.ToTable("PermissionTypes");

            entity.HasKey(pt => pt.Id);

            entity.Property(pt => pt.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(pt => pt.Description)
                .HasMaxLength(500);

            entity.Property(pt => pt.RequiresDocument)
                .HasColumnName("RequiresApproval");
        });

        modelBuilder.Entity<PermissionRequest>(entity =>
        {
            entity.ToTable("PermissionRequests");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.DurationDays)
                .HasColumnName("TotalDays");

            entity.Property(p => p.DocumentUrl)
                .HasColumnName("AttachmentPath");

            entity.Property(p => p.ApprovedByUserId)
                .HasColumnName("ReviewedById");

            entity.Property(p => p.ApprovedAt)
                .HasColumnName("ReviewedAt");

            entity.Property(p => p.ApproverComments)
                .HasColumnName("ReviewComments");

            entity.HasOne(p => p.Employee)
                .WithMany(e => e.PermissionRequests)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.PermissionType)
                .WithMany(pt => pt.PermissionRequests)
                .HasForeignKey(p => p.PermissionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.ApprovedByUser)
                .WithMany()
                .HasForeignKey(p => p.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.RequestStatus)
                .WithMany(rs => rs.PermissionRequests)
                .HasForeignKey(p => p.RequestStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RequestStatus>(entity =>
        {
            entity.ToTable("RequestStatuses");

            entity.HasKey(rs => rs.Id);

            entity.Property(rs => rs.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(rs => rs.Description)
                .HasMaxLength(500);

            entity.HasIndex(rs => rs.Name)
                .IsUnique();
        });

        // =========================
        // ATTENDANCE
        // =========================
        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.ToTable("AttendanceRecords");

            entity.HasKey(a => a.Id);

            entity.HasIndex(a => new { a.EmployeeId, a.Date })
                .IsUnique();

            entity.HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.AttendanceStatus)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(a => a.AttendanceStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AttendanceStatus>(entity =>
        {
            entity.ToTable("AttendanceStatuses");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.Description)
                .HasMaxLength(500);

            entity.HasIndex(s => s.Name)
                .IsUnique();
        });

        // =========================
        // OVERTIME
        // =========================
        modelBuilder.Entity<OvertimeRecord>(entity =>
        {
            entity.ToTable("OvertimeRecords");

            entity.HasKey(o => o.Id);

            entity.HasOne(o => o.ReviewedBy)
                .WithMany()
                .HasForeignKey(o => o.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Attendance)
                .WithMany(a => a.OvertimeRecords)
                .HasForeignKey(o => o.AttendanceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(o => o.PayrollDetail)
                .WithMany()
                .HasForeignKey(o => o.PayrollDetailId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(o => o.Status)
                .HasConversion<int>();

            entity.Property(o => o.DetectionType)
                .HasConversion<int>();
        });

        // =========================
        // PAYROLL
        // =========================
        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.ToTable("Payrolls");

            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.ProcessedBy)
                .WithMany()
                .HasForeignKey(p => p.ProcessedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.ApprovedBy)
                .WithMany()
                .HasForeignKey(p => p.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.PayrollStatus)
                .WithMany(s => s.Payrolls)
                .HasForeignKey(p => p.PayrollStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.PayrollPeriodType)
                .WithMany(t => t.Payrolls)
                .HasForeignKey(p => p.PayrollPeriodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => new
            {
                p.PeriodYear,
                p.PeriodMonth,
                p.PayrollPeriodTypeId
            }).IsUnique();
        });

        modelBuilder.Entity<PayrollStatus>(entity =>
        {
            entity.ToTable("PayrollStatuses");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.Description)
                .HasMaxLength(500);

            entity.HasIndex(s => s.Name)
                .IsUnique();
        });

        modelBuilder.Entity<PayrollPeriodType>(entity =>
        {
            entity.ToTable("PayrollPeriodTypes");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(t => t.Description)
                .HasMaxLength(500);

            entity.HasIndex(t => t.Name)
                .IsUnique();
        });

        modelBuilder.Entity<PayrollDetail>(entity =>
        {
            entity.ToTable("PayrollDetails");

            entity.HasKey(pd => pd.Id);

            entity.HasIndex(pd => new { pd.PayrollId, pd.EmployeeId })
                .IsUnique();

            entity.HasOne(pd => pd.Payroll)
                .WithMany(p => p.Details)
                .HasForeignKey(pd => pd.PayrollId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pd => pd.Employee)
                .WithMany()
                .HasForeignKey(pd => pd.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PayrollDeduction>(entity =>
        {
            entity.ToTable("PayrollDeductions");

            entity.HasKey(pd => pd.Id);

            entity.HasOne(pd => pd.PayrollDetail)
                .WithMany(d => d.Deductions)
                .HasForeignKey(pd => pd.PayrollDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pd => pd.DeductionType)
                .WithMany(dt => dt.PayrollDeductions)
                .HasForeignKey(pd => pd.DeductionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PayrollBenefit>(entity =>
        {
            entity.ToTable("PayrollBenefits");

            entity.HasKey(pb => pb.Id);

            entity.HasOne(pb => pb.PayrollDetail)
                .WithMany(d => d.Benefits)
                .HasForeignKey(pb => pb.PayrollDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pb => pb.BenefitType)
                .WithMany(bt => bt.PayrollBenefits)
                .HasForeignKey(pb => pb.BenefitTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeductionType>(entity =>
        {
            entity.ToTable("DeductionTypes");

            entity.HasKey(dt => dt.Id);

            entity.Property(dt => dt.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(dt => dt.Description)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<BenefitType>(entity =>
        {
            entity.ToTable("BenefitTypes");

            entity.HasKey(bt => bt.Id);

            entity.Property(bt => bt.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(bt => bt.Description)
                .HasMaxLength(500);
        });

        // =========================
        // SETTLEMENTS
        // =========================
        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.ToTable("Settlements");

            entity.HasKey(s => s.Id);

            entity.HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.CalculatedBy)
                .WithMany()
                .HasForeignKey(s => s.CalculatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.ApprovedBy)
                .WithMany()
                .HasForeignKey(s => s.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.TerminationType)
                .WithMany(t => t.Settlements)
                .HasForeignKey(s => s.TerminationTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(s => s.Status)
                .HasConversion<int>();
        });

        modelBuilder.Entity<TerminationType>(entity =>
        {
            entity.ToTable("TerminationTypes");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(t => t.Description)
                .HasMaxLength(500);

            entity.HasIndex(t => t.Name)
                .IsUnique();
        });

        modelBuilder.Entity<SettlementDeduction>(entity =>
        {
            entity.ToTable("SettlementDeductions");

            entity.HasKey(sd => sd.Id);

            entity.Property(sd => sd.Description)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasOne(sd => sd.Settlement)
                .WithMany(s => s.Deductions)
                .HasForeignKey(sd => sd.SettlementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================
        // ANNUAL BONUS
        // =========================
        modelBuilder.Entity<AnnualBonus>(entity =>
        {
            entity.ToTable("AnnualBonuses");

            entity.HasKey(ab => ab.Id);

            entity.HasOne(ab => ab.CalculatedBy)
                .WithMany()
                .HasForeignKey(ab => ab.CalculatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ab => ab.ApprovedBy)
                .WithMany()
                .HasForeignKey(ab => ab.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(ab => ab.Year)
                .IsUnique();

            entity.Property(ab => ab.Status)
                .HasConversion<int>();
        });

        modelBuilder.Entity<AnnualBonusDetail>(entity =>
        {
            entity.ToTable("AnnualBonusDetails");

            entity.HasKey(abd => abd.Id);

            entity.HasIndex(abd => new { abd.AnnualBonusId, abd.EmployeeId })
                .IsUnique();

            entity.HasOne(abd => abd.AnnualBonus)
                .WithMany(ab => ab.Details)
                .HasForeignKey(abd => abd.AnnualBonusId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(abd => abd.Employee)
                .WithMany()
                .HasForeignKey(abd => abd.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // PERFORMANCE EVALUATIONS
        // =========================
        modelBuilder.Entity<PerformanceEvaluation>(entity =>
        {
            entity.ToTable("PerformanceEvaluations");

            entity.HasKey(pe => pe.Id);

            entity.HasOne(pe => pe.Evaluator)
                .WithMany()
                .HasForeignKey(pe => pe.EvaluatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(pe => pe.Status)
                .HasConversion<int>();
        });

        // =========================
        // DISABILITY REQUESTS
        // =========================
        modelBuilder.Entity<DisabilityRequest>(entity =>
        {
            entity.ToTable("DisabilityRequests");

            entity.HasKey(dr => dr.Id);

            entity.HasOne(dr => dr.Employee)
                .WithMany()
                .HasForeignKey(dr => dr.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dr => dr.ReviewedBy)
                .WithMany()
                .HasForeignKey(dr => dr.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dr => dr.DisabilityType)
                .WithMany(dt => dt.DisabilityRequests)
                .HasForeignKey(dr => dr.DisabilityTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dr => dr.RequestStatus)
                .WithMany(rs => rs.DisabilityRequests)
                .HasForeignKey(dr => dr.RequestStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DisabilityType>(entity =>
        {
            entity.ToTable("DisabilityTypes");

            entity.HasKey(dt => dt.Id);

            entity.Property(dt => dt.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(dt => dt.Description)
                .HasMaxLength(500);

            entity.HasIndex(dt => dt.Name)
                .IsUnique();
        });

        // =========================
        // AUDIT LOGS
        // =========================
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            entity.HasKey(a => a.Id);

            entity.HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================
        // NOTIFICATIONS
        // =========================
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");

            entity.HasKey(n => n.Id);

            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScheduleDay>(entity =>
        {
            entity.ToTable("ScheduleDays");
            entity.HasKey(sd => sd.Id);
            entity.HasOne(sd => sd.Schedule)
                .WithMany()
                .HasForeignKey(sd => sd.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // =========================
        // DECIMAL PRECISION GLOBAL
        // =========================
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }
    }
}