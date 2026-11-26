using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("departments")]
[ApiController]
public class DepartmentController : ControllerBase
{
    private readonly DepartmentService _departmentService;
    private readonly ILogger<DepartmentController> _logger;

    public DepartmentController(DepartmentService departmentService, ILogger<DepartmentController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] Department department)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdDepartment = await _departmentService.CreateAsync(department);
            return CreatedAtAction(nameof(GetDepartment), new { id = createdDepartment.Id }, createdDepartment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return BadRequest(new { Message = ex.Message });
        }
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDepartment(Guid id)
    {
        var department = await _departmentService.GetAsync(id);
        if (department == null)
        {
            return NotFound(new { Message = "Department not found." });
        }
        return Ok(department);
    }
    [HttpGet]
    public async Task<IActionResult> ListDepartments()
    {
        var department = await _departmentService.ListAsync();
        return Ok(department);
    }
}
