namespace SigepDomain.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    // Antes era: public UserRole Role { get; set; }
    // Ahora es FK hacia la tabla UserRoles
    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Relación con empleado
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    // Relación con rol
    public UserRole? Role { get; set; }

    // Navegación
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
