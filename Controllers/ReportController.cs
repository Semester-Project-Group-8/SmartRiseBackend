using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/preset")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly ILogger<ReportController> _logger;
    private readonly ReportService _service;

    public ReportController(ReportService service, ILogger<ReportController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var reports = await _service.List();
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _service.Get(id);
        if (user == null)
        {
            return NotFound(new { Message = "Report not found." });
        }
        return Ok(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Report report)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdReport = await _service.Create(report);
            return CreatedAtAction(nameof(Get), new { id = createdReport.Id }, createdReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return BadRequest(new { Message = ex.Message });
        }
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.Delete(id);
        if (!success)
        {
            return NotFound(new { Message = "Report not found." });
        }

        return Ok(new { Message = "Report deleted successfully." });
    }
    
}