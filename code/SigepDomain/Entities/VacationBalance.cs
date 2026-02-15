namespace SigepDomain.Entities;

public class VacationBalance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal PendingDays { get; set; }
    public decimal AvailableDays => TotalDays - UsedDays - PendingDays;
    public decimal CarriedOverDays { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public Employee? Employee { get; set; }
}
