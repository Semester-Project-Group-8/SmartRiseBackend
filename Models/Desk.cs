namespace Backend.Models;

public class Desk
{
    public Guid Id { get; set; }
    public int Minor { get; set; } = -1;
    public int BeaconId { get; set; } = -1;
    public string ExternalDeskId { get; set; } = string.Empty;
    public float PreferredHeight { get; set; } = 1.0f;
    public float CompanyHeight { get; set; } = 1.0f;
    public Guid? EmployeeId { get; set; }
    public Guid DepartmentId { get; set; }
    public Employee? Employee { get; set; }
    public Department? Department { get; set; }
}

