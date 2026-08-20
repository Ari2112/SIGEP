namespace SigepDomain.Entities;

public class PublicHoliday
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsMandatoryPayment { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}