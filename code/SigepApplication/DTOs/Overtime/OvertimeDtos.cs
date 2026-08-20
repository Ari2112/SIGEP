namespace SigepApplication.DTOs.Overtime;

public class OvertimeRecordDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int? AttendanceId { get; set; }
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal MultiplierRate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DetectionType { get; set; } = string.Empty;
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    public string? Justification { get; set; }
    public DateTime? JustifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewOvertimeDto
{
    public bool Approve { get; set; }
    public string? Comments { get; set; }
}

public class JustifyOvertimeDto
{
    public string Justification { get; set; } = string.Empty;
}

public class OvertimeFilterDto
{
    public int? EmployeeId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Status { get; set; }
}