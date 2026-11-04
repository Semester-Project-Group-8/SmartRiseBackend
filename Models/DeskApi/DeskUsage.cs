using System.Text.Json.Serialization;

namespace Backend.Models.DeskApi;

public class DeskUsage
{
    [JsonPropertyName("activationsCounter")]
    public int ActivationsCounter { get; set; }

    [JsonPropertyName("sitStandCounter")]
    public int SitStandCounter { get; set; }
}