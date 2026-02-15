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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuración de Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasOne(e => e.Supervisor)
                .WithMany()
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.IdentificationNumber).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Ignore(e => e.FullName);
        });
        
        // Configuración de User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });
        
        // Configuración de VacationBalance (columna calculada)
        modelBuilder.Entity<VacationBalance>(entity =>
        {
            entity.Property(v => v.AvailableDays)
                .HasComputedColumnSql("[TotalDays] - [UsedDays] - [PendingDays]", stored: true);
            entity.HasIndex(v => new { v.EmployeeId, v.Year }).IsUnique();
        });
        
        // Configuración de VacationRequest
        modelBuilder.Entity<VacationRequest>(entity =>
        {
            entity.HasOne(v => v.ApprovedByUser)
                .WithMany()
                .HasForeignKey(v => v.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configuración de PermissionRequest
        modelBuilder.Entity<PermissionRequest>(entity =>
        {
            entity.HasOne(p => p.ApprovedByUser)
                .WithMany()
                .HasForeignKey(p => p.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configuración de VacationRequestHistory
        modelBuilder.Entity<VacationRequestHistory>(entity =>
        {
            entity.HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Aplicar configuraciones adicionales
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
