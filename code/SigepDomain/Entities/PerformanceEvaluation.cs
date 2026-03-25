namespace SigepDomain.Entities;

public class PerformanceEvaluation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int EvaluatorId { get; set; }
    public DateTime EvaluationDate { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
    public int Score { get; set; } // 3-10
    public string? Comments { get; set; }
    public string? Strengths { get; set; }
    public string? AreasToImprove { get; set; }
    public string? Goals { get; set; }
    public EvaluationStatus Status { get; set; } = EvaluationStatus.Borrador;
    public bool EmployeeAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public Employee? Employee { get; set; }
    public User? Evaluator { get; set; }
}

public enum EvaluationStatus
{
    Borrador = 1,
    Completada = 2,
    RevisadaPorEmpleado = 3
}
