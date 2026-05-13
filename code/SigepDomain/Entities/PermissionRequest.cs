namespace SigepDomain.Entities;

public class PermissionRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int PermissionTypeId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public bool IsPartialDay { get; set; }

    public decimal DurationDays { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int RequestStatusId { get; set; } = 1;

    public string? DocumentUrl { get; set; }

    public int? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? ApproverComments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Employee? Employee { get; set; }

    public PermissionType? PermissionType { get; set; }

    public RequestStatus? RequestStatus { get; set; }

    public User? ApprovedByUser { get; set; }
}