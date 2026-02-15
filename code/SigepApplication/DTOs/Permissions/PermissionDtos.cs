using SigepDomain.Enums;

namespace SigepApplication.DTOs.Permissions;

public class PermissionRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int PermissionTypeId { get; set; }
    public string PermissionTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsPartialDay { get; set; }
    public decimal DurationDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverComments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PermissionTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MaxDaysPerYear { get; set; }
    public bool RequiresDocument { get; set; }
    public bool IsPaid { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePermissionRequestDto
{
    public int EmployeeId { get; set; }
    public int PermissionTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsPartialDay { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
}

public class PermissionRequestFilterDto
{
    public int? EmployeeId { get; set; }
    public int? PermissionTypeId { get; set; }
    public RequestStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class PermissionUsageSummaryDto
{
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public Dictionary<string, decimal> UsageByType { get; set; } = new();
    public decimal TotalDaysUsed { get; set; }
}
