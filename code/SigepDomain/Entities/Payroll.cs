namespace SigepDomain.Entities;

public class Payroll
{
    public int Id { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public PayrollPeriodType PeriodType { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Borrador;
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalBenefits { get; set; }
    public decimal TotalNetSalary { get; set; }
    public int TotalEmployees { get; set; }
    public int? ProcessedById { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public User? ProcessedBy { get; set; }
    public User? ApprovedBy { get; set; }
    public ICollection<PayrollDetail> Details { get; set; } = new List<PayrollDetail>();
}

public class PayrollDetail
{
    public int Id { get; set; }
    public int PayrollId { get; set; }
    public int EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    public int WorkedDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalBenefits { get; set; }
    public decimal NetSalary { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public Payroll? Payroll { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<PayrollDeduction> Deductions { get; set; } = new List<PayrollDeduction>();
    public ICollection<PayrollBenefit> Benefits { get; set; } = new List<PayrollBenefit>();
}

public class PayrollDeduction
{
    public int Id { get; set; }
    public int PayrollDetailId { get; set; }
    public int DeductionTypeId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public decimal? PercentageValue { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PayrollDetail? PayrollDetail { get; set; }
    public DeductionType? DeductionType { get; set; }
}

public class PayrollBenefit
{
    public int Id { get; set; }
    public int PayrollDetailId { get; set; }
    public int BenefitTypeId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public decimal? PercentageValue { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PayrollDetail? PayrollDetail { get; set; }
    public BenefitType? BenefitType { get; set; }
}

public class DeductionType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPercentage { get; set; }
    public decimal DefaultValue { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class BenefitType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPercentage { get; set; }
    public decimal DefaultValue { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum PayrollStatus
{
    Borrador = 1,
    Procesando = 2,
    Completada = 3,
    Anulada = 4
}

public enum PayrollPeriodType
{
    PrimeraQuincena = 1,
    SegundaQuincena = 2,
    Mensual = 3
}
