using Backend.Controllers;
using Backend.Database;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ILogger<UserService> logger, PasswordService passwordService)
    {
        _context = context;
        _logger = logger;
        _passwordService = passwordService;
    }

    public async Task<List<Employee>> ListAsync()
    {
        return await _context.Employees
            .Include(e => e.Desk)
            .ToListAsync();
    }

    public async Task<Employee?> GetAsync(Guid id)
    {
        return await _context.Employees
            .Include(e => e.Desk)
            .FirstOrDefaultAsync(e => e.Id == id);
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
        var existingEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == employee.Email);

        if (existingEmployee != null)
        {
            existingEmployee.Name = string.IsNullOrWhiteSpace(employee.Name) ? existingEmployee.Name : employee.Name;
            existingEmployee.DateOfBirth = employee.DateOfBirth != DateOnly.MinValue ? employee.DateOfBirth : existingEmployee.DateOfBirth;
            if (string.IsNullOrWhiteSpace(existingEmployee.PasswordHash) && !string.IsNullOrEmpty(employee.PasswordHash))
            {
                employee.PasswordHash = _passwordService.HashPassword(employee, employee.PasswordHash);
            }
            existingEmployee.Role = string.IsNullOrWhiteSpace(employee.Role) ? existingEmployee.Role : employee.Role;
            existingEmployee.AccountStatus = string.IsNullOrWhiteSpace(employee.AccountStatus) ? existingEmployee.AccountStatus : "active";

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated preapproved user {Email} with ID {Id}", existingEmployee.Email, existingEmployee.Id);
            return existingEmployee;
        }

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
        if (!string.IsNullOrEmpty(employee.PasswordHash))
        {
            employee.PasswordHash = _passwordService.HashPassword(employee, employee.PasswordHash);
        }
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created user {Email} with ID {Id}", employee.Email, employee.Id);
        return employee;
    }

    public async Task<Admin?> CreateAsync(Admin admin)
    {
        if(await _context.Employees.AnyAsync(e => e.Email == admin.Email) || await _context.Admins.AnyAsync(e => e.Email == admin.Email))
        {
            _logger.LogInformation("Superuser creation failed. Email is already assigned to another user.");
            return null;
        }
        if (admin.Id == Guid.Empty)
        {
            admin.Id = Guid.NewGuid();
        }
        if (!string.IsNullOrWhiteSpace(admin.PasswordHash))
        {
            admin.PasswordHash = _passwordService.HashPassword(admin, admin.PasswordHash);
        }
        else
        {
            _logger.LogInformation("Superuser creation failed. Password is required.");
            return null;
        }
        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created superuser {Email} with ID {Id}", admin.Email, admin.Id);
        return admin;
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

    public async Task<bool> ApproveUserAsync(Guid id,Guid? deskGuid,List<Guid>? departmentGuids)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;
        employee.AccountStatus = "active";
        if(deskGuid!=null)
        {
            employee.Role = "default";
            var desk= await _context.Desks.FindAsync(deskGuid);
            if (desk == null)
            {
                _logger.LogInformation("Desk -  {deskGuid} - not found.", deskGuid);
                return false;
            }
            desk.EmployeeId = employee.Id;
        }
        else if(departmentGuids!=null) 
        {
            employee.Role = "cleaner";

            foreach(var departmentId in departmentGuids)
            {
                var department = await _context.Departments.FindAsync(departmentId);
                if(department == null)
                {
                    _logger.LogInformation("Department -  {departmentId} - not found." , departmentId);
                    return false;
                }    
                employee.WorksIn.Add(new WorksIn
                {
                    EmployeeId = employee.Id,
                    DepartmentId = department.Id,
                    StartTime = department.StartTime.AddHours(-2),
                    EndTime=department.StartTime.AddHours(-1)
                });
            }
        }
        else
        {
            _logger.LogInformation("Missing user information.");
            return false;
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("Approved user {Id}", id);
        return true;
    }
    public async Task<bool> ApproveAdminAsync(Guid id,List<Guid>? departmentGuids)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null || departmentGuids==null) return false;

        Admin admin = new Admin
        {
            Name=employee.Name,
            PasswordHash=employee.PasswordHash,
            DateOfBirth=employee.DateOfBirth,
            Email=employee.Email,
        };

        _context.Admins.Add(admin);

        foreach (var departmentId in departmentGuids)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                _logger.LogInformation("Department -  {departmentId} - not found.", departmentId);
                return false;
            }
            department.Admin = admin;
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Approved superuser {Id}", id);
        return true;
    }

    public async Task<bool> RejectAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        await DeleteAsync(id);

        _logger.LogInformation("Rejected user {Id}", id);
        return true;
    }

    public async Task<int> ApproveBatchAsync(List<string> emails)
    {
        if (emails == null || !emails.Any())
        {
            return 0;
        }

        int processedCount = 0;

        foreach (var email in emails)
        {
            if (string.IsNullOrWhiteSpace(email)) continue;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email);

            if (employee != null)
            {
                employee.AccountStatus = "active";
                processedCount++;
            }
            else
            {
                var newEmployee = new Employee
                {
                    Id = Guid.NewGuid(),
                    Email = email.Trim(),
                    AccountStatus = "active",
                    Name = "",
                    Role = "default",
                    DateOfBirth = DateOnly.MinValue,
                    PasswordHash = ""
                };

                _context.Employees.Add(newEmployee);
                processedCount++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Batch approved/created {Count} users", processedCount);
        return processedCount;
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

    public async Task<object?> GetDesk(Guid id)
    {
        var user = await _context.Employees
            .Include(e => e.Desk)
            .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (user?.Desk?.Department == null) return null;

        var desk = new
        {
            user.Desk.Minor,
            user.Desk.Department.Major
        };

        return desk;
    }
}
