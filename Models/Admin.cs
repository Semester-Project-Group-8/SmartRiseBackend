namespace Backend.Models;

public class Admin
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public DateOnly DateOfBirth { get; set; } = DateOnly.MinValue;
    public string Email { get; set; } = "";
    public string PasswordHash {  get; set; } = "";
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}