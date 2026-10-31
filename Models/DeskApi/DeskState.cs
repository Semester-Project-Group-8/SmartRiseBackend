using System.Text.Json.Serialization;

namespace Backend.Models.DeskApi;

public class DeskState
{
    [JsonPropertyName("position_mm")]
    public int PositionMm { get; set; }

    [JsonPropertyName("speed_mms")]
    public int SpeedMms { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("isPositionLost")]
    public bool IsPositionLost { get; set; }

    [JsonPropertyName("isOverloadProtectionUp")]
    public bool IsOverloadProtectionUp { get; set; }

    [JsonPropertyName("isOverloadProtectionDown")]
    public bool IsOverloadProtectionDown { get; set; }

    [JsonPropertyName("isAntiCollision")]
    public bool IsAntiCollision { get; set; }
}