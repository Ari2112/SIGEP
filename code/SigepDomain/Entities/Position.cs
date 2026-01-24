namespace SigepDomain.Entities;

public class Position
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navegación
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
