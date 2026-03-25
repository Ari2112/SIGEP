namespace SigepDomain.Entities;

public class DisabilityRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public DisabilityType Type { get; set; }
    public string? Diagnosis { get; set; }
    public string? DoctorName { get; set; }
    public string? MedicalCenter { get; set; }
    public string? DocumentNumber { get; set; }
    public string? AttachmentPath { get; set; }
    public DisabilityStatus Status { get; set; } = DisabilityStatus.Pendiente;
    public int? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public Employee? Employee { get; set; }
    public User? ReviewedBy { get; set; }
}

public enum DisabilityType
{
    EnfermedadComun = 1,
    AccidenteLaboral = 2,
    Maternidad = 3,
    Otro = 4
}

public enum DisabilityStatus
{
    Pendiente = 1,
    Aprobada = 2,
    Rechazada = 3
}
