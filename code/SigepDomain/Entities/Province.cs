namespace SigepDomain.Entities;

public class Province
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Canton> Cantons { get; set; } = new List<Canton>();

    public ICollection<EmployeeAddress> EmployeeAddresses { get; set; } = new List<EmployeeAddress>();
}