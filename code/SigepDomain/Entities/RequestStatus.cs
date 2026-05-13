namespace SigepDomain.Entities;

public class RequestStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<VacationRequest> VacationRequests { get; set; } = new List<VacationRequest>();

    public ICollection<PermissionRequest> PermissionRequests { get; set; } = new List<PermissionRequest>();

    public ICollection<DisabilityRequest> DisabilityRequests { get; set; } = new List<DisabilityRequest>();

    public ICollection<VacationRequestHistory> VacationRequestHistories { get; set; } = new List<VacationRequestHistory>();
}