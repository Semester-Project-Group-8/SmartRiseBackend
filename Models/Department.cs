namespace Backend.Models;

public class Department
{
    public Guid Id { get; set; }
    public int Major { get; set; } = -1;
    public TimeOnly StartTime { get; set; } = TimeOnly.MinValue;
    public TimeOnly EndTime { get; set; } = TimeOnly.MaxValue;
    public Guid AdminId { get; set; }
    public Admin? Admin { get; set; }
    public ICollection<Desk> Desks { get; set; } = new List<Desk>();
    public ICollection<WorksIn> WorksIn { get; set; } = new List<WorksIn>();
}