namespace Backend.Models;

public class Cleaner
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}