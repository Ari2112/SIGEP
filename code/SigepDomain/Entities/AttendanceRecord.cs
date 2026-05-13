namespace SigepDomain.Entities;

public class AttendanceRecord
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime Date { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public decimal? WorkedHours { get; set; }

    // Antes era Status con enum.
    // Ahora es FK hacia la tabla AttendanceStatuses.
    public int AttendanceStatusId { get; set; } = 1;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public Employee? Employee { get; set; }

    public AttendanceStatus? AttendanceStatus { get; set; }

    public ICollection<OvertimeRecord> OvertimeRecords { get; set; } = new List<OvertimeRecord>();
}