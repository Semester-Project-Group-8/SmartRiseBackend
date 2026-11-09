using System.Security.Principal;
using System.Text.Json.Serialization;

namespace Backend.Models.Login;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}