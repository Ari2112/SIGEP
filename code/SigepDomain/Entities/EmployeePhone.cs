namespace SigepDomain.Entities;

public class EmployeePhone
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int PhoneTypeId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Employee? Employee { get; set; }

    public PhoneType? PhoneType { get; set; }
}