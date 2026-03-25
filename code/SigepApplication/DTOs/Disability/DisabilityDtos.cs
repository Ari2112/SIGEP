namespace SigepApplication.DTOs.Disability;

public class DisabilityRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? DoctorName { get; set; }
    public string? MedicalCenter { get; set; }
    public string? DocumentNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDisabilityDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Type { get; set; }
    public string? Diagnosis { get; set; }
    public string? DoctorName { get; set; }
    public string? MedicalCenter { get; set; }
    public string? DocumentNumber { get; set; }
}

public class ReviewDisabilityDto
{
    public bool Approve { get; set; }
    public string? Comments { get; set; }
}

public class DisabilityFilterDto
{
    public int? EmployeeId { get; set; }
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
