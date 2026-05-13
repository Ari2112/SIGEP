namespace SigepDomain.Entities;

public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string IdentificationNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public DateTime HireDate { get; set; }

    public DateTime? TerminationDate { get; set; }

    public decimal BaseSalary { get; set; }

    public int EmployeeStatusId { get; set; } = 1;

    public int VacationDaysPerYear { get; set; } = 14;

    public int? PositionId { get; set; }

    public Position? Position { get; set; }

    public int? ScheduleId { get; set; }

    public Schedule? Schedule { get; set; }

    public int? SupervisorId { get; set; }

    public Employee? Supervisor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Relaciones nuevas
    public EmployeeStatus? EmployeeStatus { get; set; }

    public ICollection<EmployeePhone> EmployeePhones { get; set; } = new List<EmployeePhone>();

    public ICollection<EmployeeAddress> EmployeeAddresses { get; set; } = new List<EmployeeAddress>();

    // Navegación
    public User? User { get; set; }

    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();

    public ICollection<VacationRequest> VacationRequests { get; set; } = new List<VacationRequest>();

    public ICollection<VacationBalance> VacationBalances { get; set; } = new List<VacationBalance>();

    public ICollection<PermissionRequest> PermissionRequests { get; set; } = new List<PermissionRequest>();

    public string FullName => $"{FirstName} {LastName}";
}