namespace SigepDomain.Entities;

public class Payroll
{
    public int Id { get; set; }

    public int PeriodYear { get; set; }

    public int PeriodMonth { get; set; }

    // Antes era: PayrollPeriodType PeriodType
    // Ahora es FK hacia la tabla PayrollPeriodTypes
    public int PayrollPeriodTypeId { get; set; }

    public DateTime PeriodStartDate { get; set; }

    public DateTime PeriodEndDate { get; set; }

    // Antes era: PayrollStatus Status
    // Ahora es FK hacia la tabla PayrollStatuses
    public int PayrollStatusId { get; set; } = 1;

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

    public PayrollStatus? PayrollStatus { get; set; }

    public PayrollPeriodType? PayrollPeriodType { get; set; }

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

    // === Incapacidades (Código de Trabajo / CCSS / INS) ===
    // Días de incapacidad que cayeron dentro del período de pago.
    public int DisabilityDays { get; set; }
    // Monto rebajado del salario por los días incapacitados.
    public decimal DisabilityDeduction { get; set; }
    // Lo que el patrono sí pagó (50% de los primeros 3 días en enfermedad común).
    public decimal DisabilityEmployerPay { get; set; }
    // Subsidio informativo que la CCSS o el INS depositará (NO lo paga la planilla).
    public decimal DisabilitySubsidyAmount { get; set; }
    // Entidad que paga el subsidio: "CCSS", "INS" o "CCSS/INS".
    public string? DisabilitySubsidyEntity { get; set; }

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

    // Navegación
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

    // Navegación
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

    public ICollection<PayrollDeduction> PayrollDeductions { get; set; } = new List<PayrollDeduction>();
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

    public ICollection<PayrollBenefit> PayrollBenefits { get; set; } = new List<PayrollBenefit>();
}