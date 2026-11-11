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

    public async Task<bool> ProcessDetectionAsync(DeskDetection detection)
    {
        _logger.LogInformation("Desk '{Beacon}' detected for user '{User}'", 
            detection.BeaconName, detection.UserId);

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
}