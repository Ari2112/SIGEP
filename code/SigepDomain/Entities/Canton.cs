namespace SigepDomain.Entities;

public class Canton
{
    public int Id { get; set; }

    public int ProvinceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Province? Province { get; set; }

    public ICollection<District> Districts { get; set; } = new List<District>();

    public ICollection<EmployeeAddress> EmployeeAddresses { get; set; } = new List<EmployeeAddress>();
}