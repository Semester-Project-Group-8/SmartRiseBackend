using Backend.Database;

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
    
    public async Task<List<Report>> List()
    {
        return await _context.Report
            .Include(r => r.Employee)
            .Include(r => r.Desk)
            .ToListAsync();
    }

    public async Task<Report?> Get(int id)
    {
        return await _context.Report
            .Include(r => r.Employee)
            .Include(r => r.Desk)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Report> Create(Report report)
    {
        _context.Report.Add(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created report with ID {Id}", report.Id);
        return report;
    }

    public async Task<bool> Delete(int id)
    {
        var report = await _context.Report
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return false;

        _context.Report.Remove(report);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted report {Id}", id);
        return true;
    }
}