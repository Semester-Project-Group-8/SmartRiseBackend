namespace Backend.Models;

public class Department
{
    public Guid Id { get; set; }
    public TimeOnly StartTime {  get; set; }
    public TimeOnly EndTime { get; set; }
}