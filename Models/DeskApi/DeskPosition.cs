using System.Text.Json.Serialization;

namespace Backend.Models.DeskApi;

public class DeskPosition
{
    [JsonPropertyName("position_mm")]
    public int PositionMm { get; set; }
    
}