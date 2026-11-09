using Backend.Models;
using Backend.Models.DeskApi;
using Backend.Models.Login;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers;

[Route("api/login")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly LoginService _loginService;

    public LoginController(LoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.UserMail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Message = "User email address and password needed to log in." });
        }

        var token = await _loginService.Login(request);

        if (token == null)
        {
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        var response = new LoginResponse 
        { 
            Token = token 
        };

        return Ok(response);
    }
}