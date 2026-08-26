using System.Text.Json.Serialization;

namespace Finder.Bot.Models.Judge0;

public class CreateSubmissionResponse {
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}