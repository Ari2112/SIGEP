namespace SigepApplication.DTOs.AnnualBonus;

public class AnnualBonusDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TotalEmployees { get; set; }
    public string CalculatedByName { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AnnualBonusDetailDto> Details { get; set; } = new();
}

public class AnnualBonusDetailDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public int WorkedMonths { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal ProportionalAmount { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetAmount { get; set; }
    public string? Notes { get; set; }
}

public class CalculateAnnualBonusDto
{
    public int Year { get; set; }
    public string? Notes { get; set; }
}

public class ApproveAnnualBonusDto
{
    public string? Notes { get; set; }
}
