using Backend.Database;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class DepartmentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(AppDbContext context, ILogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Department>> ListAsync()
    {
        return await _context.Departments
            .Include(a => a.Admin)
            .ToListAsync();
    }

    public async Task<Department?> GetAsync(Guid id)
    {
        return await _context.Departments
            .Include(a => a.Admin)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department?> CreateAsync(Department department)
    {
        _context.Departments.Add(department);

        await _context.SaveChangesAsync();

        return department;
    }
}

