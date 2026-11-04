namespace Backend.Models
{
    public class WorksIn
    {
        public Guid EmployeeId { get; set; }
        public Guid DepartmentId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Employee? Cleaner {  get; set; }
        public Department? Department { get; set; }
    }
}
