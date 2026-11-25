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
    private readonly PasswordService _passwordService;
    public LoginService(ILogger<LoginService> logger, AppDbContext dbContext, PasswordService passwordService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _passwordService = passwordService;
    }

    public async Task<string?> Login(LoginRequest request)
    {
        Admin? admin = await _dbContext.Admins.FirstOrDefaultAsync(a => a.Email == request.UserMail);
        if (admin != null && _passwordService.VerifyPassword(admin, admin.PasswordHash, request.Password))
        {
            _logger.LogInformation($"Admin {request.UserMail} logged in successfully.");
            return GenerateJwtToken(request.UserMail, isAdmin: true);
        }

        Employee? employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Email == request.UserMail);
        if (employee != null && _passwordService.VerifyPassword(employee, employee.PasswordHash, request.Password))
        {
            _logger.LogInformation($"Employee {request.UserMail} logged in successfully.");
            return GenerateJwtToken(request.UserMail, isAdmin: false);
        }

        _logger.LogWarning($"Login failed: User with email {request.UserMail} not found or password incorrect.");
        return null;
    }

    private string GenerateJwtToken(string email, bool isAdmin)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TokenSecurityPasscode123456789_32chars")); 
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "Employee")
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