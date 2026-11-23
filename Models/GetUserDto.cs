namespace Backend.Models;

public class GetUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; } = "";
    public string AccountStatus { get; set; } = "";
    public string Role { get; set; } = "";
}