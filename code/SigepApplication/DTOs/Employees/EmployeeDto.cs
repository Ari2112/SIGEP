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
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public string? ScheduleName { get; set; }
}
