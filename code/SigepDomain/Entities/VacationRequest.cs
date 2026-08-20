namespace SigepDomain.Entities;

public class VacationRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int RequestedDays { get; set; }

    public int RequestStatusId { get; set; } = 1;

    public string? Reason { get; set; }

    public int? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? ApproverComments { get; set; }

    public int Version { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Employee? Employee { get; set; }

    public RequestStatus? RequestStatus { get; set; }

    public User? ApprovedByUser { get; set; }

    public ICollection<VacationRequestHistory> History { get; set; } = new List<VacationRequestHistory>();
}