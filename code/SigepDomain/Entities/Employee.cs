using SigepDomain.Enums;

namespace SigepDomain.Entities;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Activo;
    
    // Relaciones
    public int? PositionId { get; set; }
    public Position? Position { get; set; }
    
    public int? ScheduleId { get; set; }
    public Schedule? Schedule { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
