using System.Text.Json.Serialization;

namespace Backend.Models.DeskApi;

public class DeskOwnershipStatus
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }
}