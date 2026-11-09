using System.Text.Json.Serialization;

namespace Backend.Models.Login;

public class LoginRequest
{
    public string UserMail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

}