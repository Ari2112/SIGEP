namespace SigepDomain.Entities;

public class PerformanceEvaluation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int EvaluatorId { get; set; }
    public DateTime EvaluationDate { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
    public int Score { get; set; } // 0-10 (promedio de los criterios)

    // Criterios de evaluación (Opción A: 6 fijos, cada uno 1-10)
    public int ScorePunctuality { get; set; }      // Puntualidad
    public int ScoreObedience { get; set; }        // Acatamiento de órdenes
    public int ScoreQuality { get; set; }          // Calidad del trabajo
    public int ScoreResponsibility { get; set; }   // Responsabilidad
    public int ScoreTeamwork { get; set; }         // Trabajo en equipo
    public int ScoreCustomerService { get; set; }  // Atención al cliente

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