using Backend.Models;
using Backend.Models.DeskApi;
using Backend.Database;
using Backend.Clients;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class DeskService
{
    private readonly ILogger<DeskService> _logger;
    private readonly AppDbContext _dbContext;
    private readonly DeskClient _deskClient;

    public DeskService(
        ILogger<DeskService> logger,
        AppDbContext dbContext,
        DeskClient deskClient)
    {
        _logger = logger;
        _dbContext = dbContext;
        _deskClient = deskClient;
    }

    public async Task<bool> ProcessDetectionAsync(string email, string state)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.Desk)
            .FirstOrDefaultAsync(e => e.Email == email);

        var desk = employee?.Desk;
        if (employee == null || desk == null)
        {
            _logger.LogWarning("No desk is assigned to user '{User}'.", email);
            return false;
        }

        if (string.IsNullOrWhiteSpace(desk.ExternalDeskId))
        {
            _logger.LogWarning("Desk '{DeskId}' has no external API ID.", desk.Id);
            return false;
        }

        var height = state == "entered"
            ? desk.PreferredHeight
            : desk.CompanyHeight;

        var moved = await _deskClient.SetDeskPositionAsync(
            desk.ExternalDeskId,
            (int)height);

        if (!moved)
        {
            _logger.LogWarning("Failed to move desk '{DeskId}'.", desk.Id);
            return false;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Desk '{DeskId}' moved for user '{User}'.", desk.Id, email);

        return true;
    }

    public async Task<List<Desk>> GetDesksAsync()
    {
        return await _dbContext.Desks
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Desk?> GetDeskByIdAsync(Guid deskId)
    {
        return await _dbContext.Desks
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == deskId);
    }

    public async Task<bool> SetDeskHeightAsync(string deskId, DeskPosition heightUpdate)
    {
        var result = await _deskClient.SetDeskPositionAsync(deskId, heightUpdate.PositionMm);

        if (!result)
        {
            _logger.LogWarning("Failed to update height of desk '{DeskId}'.", deskId);
            return false;
        }

        _logger.LogInformation(
            "Updated height of desk '{DeskId}' to {Height} mm.",
            deskId,
            heightUpdate.PositionMm);

        return true;
    }

    public async Task<bool> SetAllDesksHeightAsync(DeskPosition heightUpdate)
    {
        var deskIds = await _deskClient.GetAllDeskIdsAsync();

        if (deskIds == null || deskIds.Count == 0)
        {
            _logger.LogWarning("No virtual desks were found.");
            return false;
        }

        var allSucceeded = true;

        foreach (var deskId in deskIds)
        {
            var result = await SetDeskHeightAsync(deskId, heightUpdate);
            allSucceeded = allSucceeded && result;
        }

        return allSucceeded;
    }

    public async Task<bool> AssignDeskAsync(Guid deskId, DeskOwnershipStatus assignment)
    {
        var desk = await _dbContext.Desks
            .FirstOrDefaultAsync(d => d.Id == deskId);

        var employeeExists = await _dbContext.Employees
            .AnyAsync(e => e.Id == assignment.UserId);

        if (desk == null || !employeeExists)
        {
            _logger.LogWarning(
                "Cannot assign desk '{DeskId}' to user '{UserId}'. Desk or user was not found.",
                deskId,
                assignment.UserId);
            return false;
        }

        if (desk.EmployeeId.HasValue && desk.EmployeeId != assignment.UserId)
        {
            _logger.LogWarning("Desk '{DeskId}' is already assigned.", deskId);
            return false;
        }

        var employeeHasAnotherDesk = await _dbContext.Desks
            .AnyAsync(d => d.EmployeeId == assignment.UserId && d.Id != deskId);

        if (employeeHasAnotherDesk)
        {
            _logger.LogWarning("User '{UserId}' already has another desk.", assignment.UserId);
            return false;
        }

        if (!desk.DepartmentId.HasValue || desk.Minor == null || string.IsNullOrWhiteSpace(desk.ExternalDeskId))
        {
            _logger.LogWarning(
                "Cannot assign desk '{DeskId}' because it is not fully configured.",
                deskId);
            return false;
        }

        desk.EmployeeId = assignment.UserId;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Assigning desk '{DeskId}' to user '{UserId}'.", deskId, assignment.UserId);
        return true;
    }

    public async Task<bool> UnassignDeskAsync(Guid deskId, DeskOwnershipStatus unassignment)
    {
        var desk = await _dbContext.Desks
            .FirstOrDefaultAsync(d => d.Id == deskId);

        if (desk == null || desk.EmployeeId != unassignment.UserId)
        {
            _logger.LogWarning(
                "Cannot unassign desk '{DeskId}' from user '{UserId}'. Assignment was not found.",
                deskId,
                unassignment.UserId);
            return false;
        }

        desk.EmployeeId = null;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Unassigning desk '{DeskId}' from user '{UserId}'.", deskId, unassignment.UserId);
        return true;
    }

    public async Task<int> SyncDesksAsync()
    {
        var externalIds = await _deskClient.GetAllDeskIdsAsync();

        if (externalIds == null)
        {
            return 0;
        }

        var existingDesks = await _dbContext.Desks.ToListAsync();
        var createdCount = 0;

        foreach (var externalId in externalIds)
        {
            if (string.IsNullOrWhiteSpace(externalId))
            {
                continue;
            }

            var exists = existingDesks.Any(d =>
                d.ExternalDeskId == externalId);

            if (exists)
            {
                continue;
            }

            _dbContext.Desks.Add(new Desk
            {
                Id = Guid.NewGuid(),
                ExternalDeskId = externalId,
                Minor = null,
                DepartmentId = null,
                EmployeeId = null,
                PreferredHeight = 750,
                CompanyHeight = 750
            });

            createdCount++;
        }

        await _dbContext.SaveChangesAsync();
        return createdCount;
    }

    public async Task<bool> ConfigureDeskAsync(Guid deskId, DeskConfiguration configuration)
    {
        var desk = await _dbContext.Desks
            .FirstOrDefaultAsync(d => d.Id == deskId);

        var departmentExists = await _dbContext.Departments
            .AnyAsync(d => d.Id == configuration.DepartmentId);

        if (desk == null || !departmentExists)
        {
            _logger.LogWarning(
                "Cannot configure desk '{DeskId}'. Desk or department was not found.",
                deskId);
            return false;
        }

        desk.DepartmentId = configuration.DepartmentId;
        desk.Minor = configuration.Minor;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Configured desk '{DeskId}' with department '{DepartmentId}' and minor '{Minor}'.",
            deskId,
            configuration.DepartmentId,
            configuration.Minor);
        return true;
    }
}