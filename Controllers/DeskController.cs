using Backend.Models;
using Backend.Models.DeskApi;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

[Route("api/desks")]
[ApiController]
[Authorize]
public class DeskController : ControllerBase
{
    private readonly DeskService _deskService;

    public DeskController(DeskService deskService)
    {
        _deskService = deskService;
    }

    [HttpPost("detect")]
    public async Task<IActionResult> Detect([FromBody] string state)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            return BadRequest(new { Message = "State is required." });
        }

        state = state.Trim().ToLowerInvariant();

        if (state != "entered" && state != "exited")
        {
            return BadRequest(new { Message = "State must be 'entered' or 'exited'." });
        }

        var email = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var result = await _deskService.ProcessDetectionAsync(email, state);

        if (!result)
        {
            return StatusCode(500, new { Message = "Failed" });
        }

        return Ok(new { Message = "Desk height updated successfully." });
    }

    [HttpGet]
    public async Task<IActionResult> GetDesks()
    {
        var desks = await _deskService.GetDesksAsync();
        return Ok(desks);
    }

    [HttpGet("{deskId}")]
    public async Task<IActionResult> GetDeskById(Guid deskId)
    {
        var desk = await _deskService.GetDeskByIdAsync(deskId);
        if (desk == null)
        {
            return NotFound(new { Message = "Desk not found." });
        }
        return Ok(desk);
    }

    [HttpPatch("{deskId}/height")]
    public async Task<IActionResult> SetDeskHeight(string deskId, [FromBody] DeskPosition heightUpdate)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _deskService.SetDeskHeightAsync(deskId, heightUpdate);

        if (!result)
        {
            return StatusCode(500, new { Message = "Failed to set desk height." });
        }

        return Ok(new { Message = "Desk height updated successfully." });
    }

    [HttpPost("height")]
    public async Task<IActionResult> SetAllDesksHeight([FromBody] DeskPosition heightUpdate)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _deskService.SetAllDesksHeightAsync(heightUpdate);

        if (!result)
        {
            return StatusCode(500, new { Message = "Failed to set height for all desks." });
        }

        return Ok(new { Message = "Height updated for all desks." });
    }

    [HttpPost("{deskId}/assignment")]
    public async Task<IActionResult> AssignDesk(Guid deskId, [FromBody] DeskOwnershipStatus assignment)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _deskService.AssignDeskAsync(deskId, assignment);

        if (!result)
        {
            return StatusCode(500, new { Message = "Failed to assign desk." });
        }

        return Ok(new { Message = "Desk assigned successfully." });
    }

    [HttpDelete("{deskId}/assignment")]
    public async Task<IActionResult> UnassignDesk(Guid deskId, [FromBody] DeskOwnershipStatus unassignment)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _deskService.UnassignDeskAsync(deskId, unassignment);

        if (!result)
        {
            return StatusCode(500, new { Message = "Failed to unassign desk." });
        }

        return Ok(new { Message = "Desk unassigned successfully." });
    }
    [HttpPost("sync")]
    public async Task<IActionResult> SyncDesks()
    {
        var createdCount = await _deskService.SyncDesksAsync();
        return Ok(new { CreatedCount = createdCount });
    }

    [HttpPatch("{deskId}/minor-major")]
    public async Task<IActionResult> UpdateDeskMinorMajor(Guid deskId, [FromBody] Dictionary<string, int?> body)
    {
        if (body == null)
        {
            return BadRequest(new { Message = "Request body is required." });
        }

        if (body.TryGetValue("minor", out var minor) && minor.HasValue && minor.Value < 0)
        {
            return BadRequest(new { Message = "Minor value cannot be negative." });
        }

        if (body.TryGetValue("major", out var major) && major.HasValue && major.Value < 0)
        {
            return BadRequest(new { Message = "Major value cannot be negative." });
        }

        var result = await _deskService.UpdateDeskMinorMajorAsync(deskId, body);

        if (!result)
        {
            return NotFound(new { Message = "Desk not found or no department with the specified major found." });
        }

        return Ok(new { Message = "Desk minor and department major updated successfully." });
    }
}