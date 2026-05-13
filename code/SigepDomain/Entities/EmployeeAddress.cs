namespace SigepDomain.Entities;

public class EmployeeAddress
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int AddressTypeId { get; set; }

    public int ProvinceId { get; set; }

    public int CantonId { get; set; }

    public int DistrictId { get; set; }

    public string ExactAddress { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Employee? Employee { get; set; }

    public AddressType? AddressType { get; set; }

    public Province? Province { get; set; }

    public Canton? Canton { get; set; }

    public District? District { get; set; }
}