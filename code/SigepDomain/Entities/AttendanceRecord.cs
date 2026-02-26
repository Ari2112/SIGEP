namespace SigepDomain.Entities;

public class AttendanceRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal? WorkedHours { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Parcial;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public Employee? Employee { get; set; }
    public ICollection<OvertimeRecord> OvertimeRecords { get; set; } = new List<OvertimeRecord>();
}

public enum AttendanceStatus
{
    Parcial = 1,
    Completo = 2,
    Ausente = 3,
    Permiso = 4,
    Vacaciones = 5,
    Incapacidad = 6
}
