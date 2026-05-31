namespace SigepDomain.Entities;

public class OvertimeRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int? AttendanceId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalHours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal MultiplierRate { get; set; } = 1.5m;
    public decimal TotalAmount { get; set; }
    public OvertimeStatus Status { get; set; } = OvertimeStatus.Detectada;
    public OvertimeDetectionType DetectionType { get; set; } = OvertimeDetectionType.Automatica;
    public int? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Reason { get; set; }
    public string? ReviewComments { get; set; }
    public int? PayrollDetailId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public Employee? Employee { get; set; }
    public AttendanceRecord? Attendance { get; set; }
    public User? ReviewedBy { get; set; }
    public PayrollDetail? PayrollDetail { get; set; }
}

public enum OvertimeStatus
{
    Detectada = 1,
    Pendiente = 2,
    Aprobada = 3,
    Rechazada = 4,
    Pagada = 5
}

public enum OvertimeDetectionType
{
    Automatica = 1,
    Manual = 2
}
