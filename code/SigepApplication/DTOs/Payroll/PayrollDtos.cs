namespace SigepApplication.DTOs.Payroll;

public class PayrollDto
{
    public int Id { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public string PeriodType { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalBenefits { get; set; }
    public decimal TotalNetSalary { get; set; }
    public int TotalEmployees { get; set; }
    public string? ProcessedByName { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PayrollDetailDto> Details { get; set; } = new();
}

public class PayrollDetailDto
{
    public int Id { get; set; }
    public int PayrollId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public decimal BaseSalary { get; set; }
    public int WorkedDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalBenefits { get; set; }
    public decimal NetSalary { get; set; }

    // === Incapacidades ===
    public int DisabilityDays { get; set; }
    public decimal DisabilityDeduction { get; set; }
    public decimal DisabilityEmployerPay { get; set; }
    public decimal DisabilitySubsidyAmount { get; set; }
    public string? DisabilitySubsidyEntity { get; set; }

    public string? Notes { get; set; }
    public List<DeductionItemDto> Deductions { get; set; } = new();
    public List<BenefitItemDto> Benefits { get; set; } = new();
}

public class DeductionItemDto
{
    public int Id { get; set; }
    public string DeductionTypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public decimal? PercentageValue { get; set; }
}

public class BenefitItemDto
{
    public int Id { get; set; }
    public string BenefitTypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public decimal? PercentageValue { get; set; }
}

public class CreatePayrollDto
{
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public int PeriodType { get; set; }
    public string? Notes { get; set; }
}

public class ApprovePayrollDto
{
    public string? Notes { get; set; }
}

public class DeductionTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPercentage { get; set; }
    public decimal DefaultValue { get; set; }
    public bool IsActive { get; set; }
}

public class BenefitTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPercentage { get; set; }
    public decimal DefaultValue { get; set; }
    public bool IsActive { get; set; }
}