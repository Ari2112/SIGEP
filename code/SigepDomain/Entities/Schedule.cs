namespace SigepDomain.Entities;

public class Schedule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int WorkHoursPerDay { get; set; } = 8;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navegación
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
