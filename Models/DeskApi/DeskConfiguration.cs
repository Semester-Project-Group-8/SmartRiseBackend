using System.Text.Json.Serialization;

namespace Backend.Models.DeskApi;

public class DeskConfiguration
{
    [JsonPropertyName("departmentId")]
    public Guid DepartmentId { get; set; }

    [JsonPropertyName("minor")]
    public int Minor { get; set; }
}
