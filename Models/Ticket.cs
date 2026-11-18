namespace Backend.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description_Text { get; set; } = string.Empty;
        public DateTime ReportTime { get; set; } = DateTime.MinValue;
        public Guid? EmployeeId { get; set; }
        public Guid DeskId { get; set; }
        public Employee? Employee { get; set; }
        public Desk? Desk { get; set; }
    }
}
