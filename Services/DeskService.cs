using Backend.Models;

namespace Backend.Services;

public class DeskService
{
    private readonly ILogger<DeskService> _logger;

    public DeskService(ILogger<DeskService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ProcessDetectionAsync(DeskDetection detection)
    {
        _logger.LogInformation("Desk '{Beacon}' detected for user '{User}'", 
            detection.BeaconName, detection.UserId);

        return true;
    }
}