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
    public string? Address { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal BaseSalary { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Activo;
    public int VacationDaysPerYear { get; set; } = 14;
    
    // Relaciones
    public int? PositionId { get; set; }
    public Position? Position { get; set; }
    
    public int? ScheduleId { get; set; }
    public Schedule? Schedule { get; set; }
    
    public int? SupervisorId { get; set; }
    public Employee? Supervisor { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navegación
    public User? User { get; set; }
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public ICollection<VacationRequest> VacationRequests { get; set; } = new List<VacationRequest>();
    public ICollection<VacationBalance> VacationBalances { get; set; } = new List<VacationBalance>();
    public ICollection<PermissionRequest> PermissionRequests { get; set; } = new List<PermissionRequest>();
    
    // Propiedades calculadas
    public string FullName => $"{FirstName} {LastName}";
}
