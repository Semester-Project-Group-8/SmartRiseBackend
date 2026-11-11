using Backend.Models;
using Backend.Models.DeskApi;
using Backend.Database;
using Backend.Clients;
using Microsoft.EntityFrameworkCore;
using Backend.Models.Login;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class LoginService
{
    private readonly ILogger<LoginService> _logger;
    private readonly AppDbContext _dbContext;

    public LoginService(
        ILogger<LoginService> logger,
        AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<string?> Login(LoginRequest request)
    {
        var user = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Email == request.UserMail);

        if (user == null)
        {
            _logger.LogWarning($"Login failed: User with email {request.UserMail} not found.");
            return null;
        }

        if (user.PasswordHash == request.Password)
        {
            _logger.LogInformation($"User {request.UserMail} logged in successfully.");
            return GenerateJwtToken(user.Email);
        }

        return null;
    }

    private string GenerateJwtToken(string email)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TokenSecurityPasscode123456789_32chars")); 
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "SmartRiseBackEnd", 
            audience: "SmartRiseFrontEnd",
            claims: claims,
            expires: DateTime.UtcNow.AddMonths(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}