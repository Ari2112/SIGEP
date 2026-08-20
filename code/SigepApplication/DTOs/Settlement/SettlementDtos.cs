namespace SigepApplication.DTOs.Settlement;

public class SettlementDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string TerminationType { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime TerminationDate { get; set; }
    public decimal LastSalary { get; set; }
    public decimal AverageSalary { get; set; }
    public int WorkedYears { get; set; }
    public int WorkedMonths { get; set; }
    public int WorkedDays { get; set; }
    public decimal PendingVacationDays { get; set; }
    public decimal VacationAmount { get; set; }
   public decimal ProportionalBonus { get; set; }
public decimal NoticeAmount { get; set; }
public decimal SeveranceAmount { get; set; }
    public decimal OtherBenefits { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal GrossTotal { get; set; }
    public decimal NetTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CalculatedByName { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SettlementDeductionDto> Deductions { get; set; } = new();
}

public class SettlementDeductionDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CalculateSettlementDto
{
    public int EmployeeId { get; set; }
    public int TerminationType { get; set; }
    public DateTime TerminationDate { get; set; }
    public string? Notes { get; set; }
    public List<SettlementDeductionInputDto> AdditionalDeductions { get; set; } = new();
}

public class SettlementDeductionInputDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ApproveSettlementDto
{
    public string? Notes { get; set; }
}
