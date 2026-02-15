using SigepDomain.Enums;

namespace SigepDomain.Entities;

public class VacationRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RequestedDays { get; set; }
    public string? Reason { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pendiente;
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverComments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public Employee? Employee { get; set; }
    public User? ApprovedByUser { get; set; }
    public ICollection<VacationRequestHistory> History { get; set; } = new List<VacationRequestHistory>();
}
