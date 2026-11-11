using Backend.Models;
using Backend.Models.DeskApi;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/desks")]
[ApiController]
public class DeskController : ControllerBase
{
    private readonly DeskService _deskService;

    public DeskController(DeskService deskService)
    {
        _deskService = deskService;
    }

    [HttpPost("Detect")]
    public async Task<IActionResult> Detect([FromBody] DeskDetection detection)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(detection.BeaconName) || string.IsNullOrWhiteSpace(detection.UserId))
        {
            return BadRequest(new { Message = "BeaconName and UserId are required." });
        }

        var result = await _deskService.ProcessDetectionAsync(detection);

        if (!result)
        {
            return StatusCode(500, new { Message = "Failed" });
        }

        return Ok(new { Message = "Desk detected successfully." });
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
}