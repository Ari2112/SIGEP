namespace SigepDomain.Entities;

public class DisabilityRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int TotalDays { get; set; }

    // Antes era: public DisabilityType Type { get; set; }
    // Ahora es FK hacia la tabla DisabilityTypes
    public int DisabilityTypeId { get; set; }

    public string? Diagnosis { get; set; }

    public string? DoctorName { get; set; }

    public string? MedicalCenter { get; set; }

    public string? DocumentNumber { get; set; }

    public string? AttachmentPath { get; set; }

    // Antes era: public DisabilityStatus Status { get; set; }
    // Ahora usa el catálogo general RequestStatuses
    public int RequestStatusId { get; set; } = 1;

    public int? ReviewedById { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewComments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public Employee? Employee { get; set; }

    public DisabilityType? DisabilityType { get; set; }

    public RequestStatus? RequestStatus { get; set; }

    public User? ReviewedBy { get; set; }
}