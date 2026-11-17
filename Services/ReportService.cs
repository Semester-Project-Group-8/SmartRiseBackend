using Backend.Database;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ReportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportService> _logger;
    
    public ReportService(AppDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<List<Ticket>> List()
    {
        return await _context.Tickets
            .Include(r => r.Employee)
            .Include(r => r.Desk)
            .ToListAsync();
    }

    public async Task<Ticket?> Get(Guid id)
    {
        return await _context.Tickets
            .Include(r => r.Employee)
            .Include(r => r.Desk)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Ticket> Create(Ticket report)
    {
        _context.Tickets.Add(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created report with ID {Id}", report.Id);
        return report;
    }

    public async Task<bool> Delete(Guid id)
    {
        var report = await _context.Tickets
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return false;

        _context.Tickets.Remove(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted report {Id}", id);
        return true;
    }
}