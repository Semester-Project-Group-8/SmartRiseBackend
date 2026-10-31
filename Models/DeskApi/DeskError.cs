using System.Text.Json.Serialization;

namespace Backend.Models.DeskApi;

public class DeskError
{
    [JsonPropertyName("time_s")]
    public int TimeS { get; set; }

    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; set; }
}