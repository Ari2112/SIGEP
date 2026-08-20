namespace SigepDomain.Entities;

public class District
{
    public int Id { get; set; }

    public int CantonId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Canton? Canton { get; set; }

    public ICollection<EmployeeAddress> EmployeeAddresses { get; set; } = new List<EmployeeAddress>();
}