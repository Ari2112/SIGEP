namespace SigepDomain.Entities;

public class AnnualBonus
{
    public int Id { get; set; }
    public int Year { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public AnnualBonusStatus Status { get; set; } = AnnualBonusStatus.Borrador;
    public decimal TotalAmount { get; set; }
    public int TotalEmployees { get; set; }
    public int CalculatedById { get; set; }
    public DateTime CalculatedAt { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public User? CalculatedBy { get; set; }
    public User? ApprovedBy { get; set; }
    public ICollection<AnnualBonusDetail> Details { get; set; } = new List<AnnualBonusDetail>();
}

public class AnnualBonusDetail
{
    public int Id { get; set; }
    public int AnnualBonusId { get; set; }
    public int EmployeeId { get; set; }
    public int WorkedMonths { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal ProportionalAmount { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public AnnualBonus? AnnualBonus { get; set; }
    public Employee? Employee { get; set; }
}

public enum AnnualBonusStatus
{
    Borrador = 1,
    Calculado = 2,
    Aprobado = 3,
    Pagado = 4,
    Anulado = 5
}
