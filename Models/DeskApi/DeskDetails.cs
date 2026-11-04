using System.Text.Json.Serialization;

namespace Backend.Models.DeskApi;

public class DeskDetails
{
    [JsonPropertyName("config")]
    public DeskConfig Config { get; set; } = new();

    [JsonPropertyName("state")]
    public DeskState State { get; set; } = new();

    [JsonPropertyName("usage")]
    public DeskUsage Usage { get; set; } = new();

    [JsonPropertyName("lastErrors")]
    public List<DeskError> LastErrors { get; set; } = new();
}