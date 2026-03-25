namespace SigepApplication.DTOs.PerformanceEvaluation;

public class PerformanceEvaluationDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public int EvaluatorId { get; set; }
    public string EvaluatorName { get; set; } = string.Empty;
    public DateTime EvaluationDate { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
    public int Score { get; set; }
    public string ScoreLabel { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public string? Strengths { get; set; }
    public string? AreasToImprove { get; set; }
    public string? Goals { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool EmployeeAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEvaluationDto
{
    public int EmployeeId { get; set; }
    public DateTime EvaluationDate { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
    public int Score { get; set; }
    public string? Comments { get; set; }
    public string? Strengths { get; set; }
    public string? AreasToImprove { get; set; }
    public string? Goals { get; set; }
}

public class UpdateEvaluationDto
{
    public int Score { get; set; }
    public string? Comments { get; set; }
    public string? Strengths { get; set; }
    public string? AreasToImprove { get; set; }
    public string? Goals { get; set; }
}

public class EvaluationFilterDto
{
    public int? EmployeeId { get; set; }
    public int? Year { get; set; }
    public string? Status { get; set; }
}
