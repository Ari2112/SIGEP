namespace SigepDomain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "INFO"; // INFO, WARNING, SUCCESS, ERROR
    public string? Module { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public User? User { get; set; }
}
