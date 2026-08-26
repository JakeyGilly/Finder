using System.Text.Json.Serialization;

namespace Finder.Bot.Models.Judge0;

public class SubmissionStatus {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}