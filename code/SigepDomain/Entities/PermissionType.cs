namespace SigepDomain.Entities;

public class PermissionType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? MaxDaysPerYear { get; set; }
    public bool RequiresDocument { get; set; }
    public bool IsPaid { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public ICollection<PermissionRequest> PermissionRequests { get; set; } = new List<PermissionRequest>();
}
