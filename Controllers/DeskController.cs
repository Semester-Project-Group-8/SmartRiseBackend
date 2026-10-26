using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("Desk")]
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
}