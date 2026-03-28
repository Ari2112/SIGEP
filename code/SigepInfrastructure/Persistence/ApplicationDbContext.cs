using Microsoft.EntityFrameworkCore;
using SigepDomain.Entities;

namespace SigepInfrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Entidades base
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Schedule> Schedules { get; set; }

    // Módulo transversal
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    // Módulo vacaciones
    public DbSet<VacationBalance> VacationBalances { get; set; }
    public DbSet<VacationRequest> VacationRequests { get; set; }
    public DbSet<VacationRequestHistory> VacationRequestHistory { get; set; }

    // Módulo permisos
    public DbSet<PermissionType> PermissionTypes { get; set; }
    public DbSet<PermissionRequest> PermissionRequests { get; set; }

    // Módulo asistencia
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    // Módulo horas extra
    public DbSet<OvertimeRecord> OvertimeRecords { get; set; }

    // Módulo planilla
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<PayrollDetail> PayrollDetails { get; set; }
    public DbSet<PayrollDeduction> PayrollDeductions { get; set; }
    public DbSet<PayrollBenefit> PayrollBenefits { get; set; }
    public DbSet<DeductionType> DeductionTypes { get; set; }
    public DbSet<BenefitType> BenefitTypes { get; set; }

    // Módulo liquidaciones
    public DbSet<Settlement> Settlements { get; set; }
    public DbSet<SettlementDeduction> SettlementDeductions { get; set; }

    // Módulo aguinaldo
    public DbSet<AnnualBonus> AnnualBonuses { get; set; }
    public DbSet<AnnualBonusDetail> AnnualBonusDetails { get; set; }

    // Módulo evaluación de desempeño
    public DbSet<PerformanceEvaluation> PerformanceEvaluations { get; set; }

    // Módulo incapacidades
    public DbSet<DisabilityRequest> DisabilityRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasOne(e => e.Supervisor)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.IdentificationNumber).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Ignore(e => e.FullName);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<VacationBalance>(entity =>
        {
            entity.Property(v => v.AvailableDays)
                .HasComputedColumnSql("[TotalDays] - [UsedDays] - [PendingDays]", stored: true);
            entity.Property(v => v.CarriedOverDays).HasColumnName("CarryOverDays");
            entity.HasIndex(v => new { v.EmployeeId, v.Year }).IsUnique();
        });

        modelBuilder.Entity<VacationRequest>(entity =>
        {
            entity.Property(v => v.RequestedDays).HasColumnName("TotalDays");
            entity.Property(v => v.Reason).HasColumnName("Comments");
            entity.Property(v => v.ApprovedByUserId).HasColumnName("ReviewedById");
            entity.Property(v => v.ApprovedAt).HasColumnName("ReviewedAt");
            entity.Property(v => v.ApproverComments).HasColumnName("ReviewComments");
            entity.HasOne(v => v.ApprovedByUser)
                .WithMany()
                .HasForeignKey(v => v.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PermissionType>(entity =>
        {
            entity.Property(pt => pt.RequiresDocument).HasColumnName("RequiresApproval");
        });

        modelBuilder.Entity<PermissionRequest>(entity =>
        {
            entity.Property(p => p.DurationDays).HasColumnName("TotalDays");
            entity.Property(p => p.DocumentUrl).HasColumnName("AttachmentPath");
            entity.Property(p => p.ApprovedByUserId).HasColumnName("ReviewedById");
            entity.Property(p => p.ApprovedAt).HasColumnName("ReviewedAt");
            entity.Property(p => p.ApproverComments).HasColumnName("ReviewComments");
            entity.HasOne(p => p.ApprovedByUser)
                .WithMany()
                .HasForeignKey(p => p.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VacationRequestHistory>(entity =>
        {
            entity.Property(h => h.ChangedByUserId).HasColumnName("ChangedById");
            entity.HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
            entity.Property(a => a.Status).HasConversion<int>();
        });

        modelBuilder.Entity<OvertimeRecord>(entity =>
        {
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
            entity.Property(o => o.Status).HasConversion<int>();
            entity.Property(o => o.DetectionType).HasConversion<int>();
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasOne(p => p.ProcessedBy).WithMany().HasForeignKey(p => p.ProcessedById).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.ApprovedBy).WithMany().HasForeignKey(p => p.ApprovedById).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(p => new { p.PeriodYear, p.PeriodMonth, p.PeriodType }).IsUnique();
            entity.Property(p => p.Status).HasConversion<int>();
            entity.Property(p => p.PeriodType).HasConversion<int>();
        });

        modelBuilder.Entity<PayrollDetail>(entity =>
        {
            entity.HasIndex(pd => new { pd.PayrollId, pd.EmployeeId }).IsUnique();
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasOne(s => s.CalculatedBy).WithMany().HasForeignKey(s => s.CalculatedById).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.ApprovedBy).WithMany().HasForeignKey(s => s.ApprovedById).OnDelete(DeleteBehavior.Restrict);
            entity.Property(s => s.TerminationType).HasConversion<int>();
            entity.Property(s => s.Status).HasConversion<int>();
        });

        modelBuilder.Entity<AnnualBonus>(entity =>
        {
            entity.HasOne(ab => ab.CalculatedBy).WithMany().HasForeignKey(ab => ab.CalculatedById).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ab => ab.ApprovedBy).WithMany().HasForeignKey(ab => ab.ApprovedById).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(ab => ab.Year).IsUnique();
            entity.Property(ab => ab.Status).HasConversion<int>();
        });

        modelBuilder.Entity<AnnualBonusDetail>(entity =>
        {
            entity.HasIndex(abd => new { abd.AnnualBonusId, abd.EmployeeId }).IsUnique();
        });

        modelBuilder.Entity<PerformanceEvaluation>(entity =>
        {
            entity.HasOne(pe => pe.Evaluator).WithMany().HasForeignKey(pe => pe.EvaluatorId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(pe => pe.Status).HasConversion<int>();
        });

        modelBuilder.Entity<DisabilityRequest>(entity =>
        {
            entity.HasOne(dr => dr.ReviewedBy).WithMany().HasForeignKey(dr => dr.ReviewedById).OnDelete(DeleteBehavior.Restrict);
            entity.Property(dr => dr.Type).HasConversion<int>();
            entity.Property(dr => dr.Status).HasConversion<int>();
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
