namespace SigepDomain.Entities;

public class Settlement
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public TerminationType TerminationType { get; set; }
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
    public decimal SeveranceAmount { get; set; }
    public decimal OtherBenefits { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal GrossTotal { get; set; }
    public decimal NetTotal { get; set; }
    public SettlementStatus Status { get; set; } = SettlementStatus.Borrador;
    public int CalculatedById { get; set; }
    public DateTime CalculatedAt { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public Employee? Employee { get; set; }
    public User? CalculatedBy { get; set; }
    public User? ApprovedBy { get; set; }
    public ICollection<SettlementDeduction> Deductions { get; set; } = new List<SettlementDeduction>();
}

public class SettlementDeduction
{
    public int Id { get; set; }
    public int SettlementId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Settlement? Settlement { get; set; }
}

public enum TerminationType
{
    Renuncia = 1,
    DespidoConResponsabilidad = 2,
    DespidoSinResponsabilidad = 3,
    MutuoAcuerdo = 4,
    Jubilacion = 5
}

public enum SettlementStatus
{
    Borrador = 1,
    Calculada = 2,
    Aprobada = 3,
    Pagada = 4,
    Anulada = 5
}
