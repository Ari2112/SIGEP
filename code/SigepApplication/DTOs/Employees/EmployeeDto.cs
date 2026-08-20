namespace SigepApplication.DTOs.Employees;

public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string IdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? PositionId { get; set; }
    public string? PositionName { get; set; }
    public int? ScheduleId { get; set; }
    public string? ScheduleName { get; set; }
    public int VacationDaysPerYear { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public int? PositionId { get; set; }
    public int? ScheduleId { get; set; }
    public int? SupervisorId { get; set; }
    public int VacationDaysPerYear { get; set; } = 14;
    // Credenciales del usuario a crear con el empleado
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? UserRole { get; set; } = "Empleado";
}

public class UpdateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? BirthDate { get; set; }
    public decimal BaseSalary { get; set; }
    public int? PositionId { get; set; }
    public int? ScheduleId { get; set; }
    public int? SupervisorId { get; set; }
    public string Status { get; set; } = "Activo";
    public int VacationDaysPerYear { get; set; } = 14;
}

public class PositionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? BaseSalary { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePositionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? BaseSalary { get; set; }
}

public class ScheduleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int WorkHoursPerDay { get; set; }
    public bool IsActive { get; set; }
}

public class CreateScheduleDto
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int WorkHoursPerDay { get; set; } = 8;
}
public class GeoItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
