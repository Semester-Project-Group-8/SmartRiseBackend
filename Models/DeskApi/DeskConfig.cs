using System.Text.Json.Serialization;

namespace Backend.Models.DeskApi;

public class DeskConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;
}