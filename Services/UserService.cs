using Backend.Controllers;
using Backend.Database;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Employee>> ListAsync()
    {
        return await _context.Employees
            .Include(e => e.Desk)
            .ToListAsync();
    }

    public async Task<Employee?> GetAsync(string id)
    {
        return await _context.Employees
            .Include(e => e.Desk)
            .FirstOrDefaultAsync(e => Equals(e.Id, id));
    }

    public async Task<List<Employee>> GetPendingAsync()
    {
        return await _context.Employees
            .Include(e => e.Desk)
            .Where(e => e.AccountStatus == "pending" || e.AccountStatus == "inactive")
            .ToListAsync();
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        if (employee.Id == Guid.Empty)
        {
            employee.Id = Guid.NewGuid();
        }
        if (string.IsNullOrEmpty(employee.AccountStatus))
        {
            employee.AccountStatus = "pending";
        }
        if (string.IsNullOrEmpty(employee.Role))
        {
            employee.Role = "default";
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created user {Email} with ID {Id}", employee.Email, employee.Id);
        return employee;
    }

    public async Task<bool> UpdatePresetsAsync(Guid id, Preset preset)
    {
        var employee = await _context.Employees
            .Include(e => e.Desk)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            return false;
        }

        if (employee.Desk == null)
        {
            var desk = await _context.Desks.FirstOrDefaultAsync(d => d.EmployeeId == id);
            if (desk == null)
            {
                return false;
            }
            desk.PreferredHeight = (float)preset.PresetHeight;
            await _context.SaveChangesAsync();
            return true;
        }

        employee.Desk.PreferredHeight = (float)preset.PresetHeight;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated presets for user {Id} to height {Height}", id, preset.PresetHeight);
        return true;
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        employee.AccountStatus = "active";
        await _context.SaveChangesAsync();

        _logger.LogInformation("Approved user {Id}", id);
        return true;
    }

    public async Task<bool> RejectAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        employee.AccountStatus = "inactive";
        await _context.SaveChangesAsync();

        _logger.LogInformation("Rejected user {Id}", id);
        return true;
    }

    public async Task<int> ApproveBatchAsync(List<string> emails)
    {
        var employees = await _context.Employees
            .Where(e => emails.Contains(e.Email))
            .ToListAsync();

        foreach (var emp in employees)
        {
            emp.AccountStatus = "active";
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Approved batch of {Count} users", employees.Count);
        return employees.Count;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var employee = await _context.Employees
            .Include(e => e.Desk)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return false;

        if (employee.Desk != null)
        {
            employee.Desk.EmployeeId = null;
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted user {Id}", id);
        return true;
    }
}
