using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(UserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ListUsers()
    {
        var users = await _userService.ListAsync();
        return Ok(users);
    }

    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery] string id)
    {
        var user = await _userService.GetAsync(id);
        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }
        return Ok(user);
    }

    [HttpGet("approval-pending")]
    public async Task<IActionResult> GetPendingUsers()
    {
        var users = await _userService.GetPendingAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdUser = await _userService.CreateAsync(employee);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("approve")]
    public async Task<IActionResult> ApproveBatch([FromBody] List<string> emails)
    {
        if (emails == null || !emails.Any())
        {
            return BadRequest(new { Message = "Email list cannot be empty." });
        }

        try
        {
            var count = await _userService.ApproveBatchAsync(emails);
            return Ok(new { Message = $"Approved {count} users successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving batch of users");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/presets")]
    public async Task<IActionResult> UpdatePresets(Guid id, [FromBody] Preset preset)
    {
        if (preset == null)
        {
            return BadRequest(new { Message = "Preset data is required." });
        }

        var success = await _userService.UpdatePresetsAsync(id, preset);
        if (!success)
        {
            return NotFound(new { Message = "User or associated desk not found." });
        }

        return Ok(new { Message = "Presets updated successfully." });
    }

    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> ApproveUser(Guid id)
    {
        var success = await _userService.ApproveAsync(id);
        if (!success)
        {
            return NotFound(new { Message = "User not found." });
        }

        return Ok(new { Message = "User approved successfully." });
    }

    [HttpPatch("{id}/reject")]
    public async Task<IActionResult> RejectUser(Guid id)
    {
        var success = await _userService.RejectAsync(id);
        if (!success)
        {
            return NotFound(new { Message = "User not found." });
        }

        return Ok(new { Message = "User rejected successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var success = await _userService.DeleteAsync(id);
        if (!success)
        {
            return NotFound(new { Message = "User not found." });
        }

        return Ok(new { Message = "User deleted successfully." });
    }
}

public class Preset
{
    public bool PresetValue { get; set; } // True = sit || False = stand
    public double PresetHeight { get; set; }

}
