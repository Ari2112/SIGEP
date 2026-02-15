using SigepDomain.Enums;

namespace SigepDomain.Entities;

public class VacationRequestHistory
{
    public int Id { get; set; }
    public int VacationRequestId { get; set; }
    public RequestStatus Status { get; set; }
    public string? Comments { get; set; }
    public int ChangedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public VacationRequest? VacationRequest { get; set; }
    public User? ChangedByUser { get; set; }
}
