using SigepDomain.Enums;

namespace SigepApplication.DTOs.Vacations;

public class VacationRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RequestedDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverComments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class VacationBalanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal PendingDays { get; set; }
    public decimal AvailableDays { get; set; }
    public decimal CarriedOverDays { get; set; }
    public DateTime ExpirationDate { get; set; }
}

public class CreateVacationRequestDto
{
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

public class UpdateVacationRequestDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

public class VacationRequestFilterDto
{
    public int? EmployeeId { get; set; }
    public RequestStatus? Status { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public int? Year { get; set; }
}

public class VacationRequestHistoryDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public string? ChangedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
