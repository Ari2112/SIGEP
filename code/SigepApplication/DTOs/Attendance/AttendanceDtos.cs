namespace SigepApplication.DTOs.Attendance;

public class AttendanceRecordDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal? WorkedHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Horario del empleado
    public string? ScheduleName { get; set; }
    public TimeSpan? ScheduledStartTime { get; set; }
    public TimeSpan? ScheduledEndTime { get; set; }

    // Tardía
    public bool IsLate { get; set; }
    public int LateMinutes { get; set; }

    // Horas extra
    public decimal? OvertimeHours { get; set; }
}

public class CheckInDto
{
    public string? Notes { get; set; }
}

public class CheckOutDto
{
    public string? Notes { get; set; }
    // Motivo obligatorio si sale después del horario
    public string? OvertimeReason { get; set; }
}

public class AttendanceFilterDto
{
    public int? EmployeeId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Status { get; set; }
}

// Reporte de tardías
public class LatenessReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan ScheduledStart { get; set; }
    public DateTime CheckInTime { get; set; }
    public int LateMinutes { get; set; }
}

// Reporte de asistencia general
public class AttendanceSummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public decimal TotalWorkedHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
}