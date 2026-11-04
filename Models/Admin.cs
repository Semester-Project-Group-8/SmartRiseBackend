namespace Backend.Models;

public class Admin
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; }
    public string PasswordHash {  get; set; }
}