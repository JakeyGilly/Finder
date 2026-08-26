using System.Text.Json.Serialization;


namespace Finder.Bot.Models.Judge0;

public record LanguageReponse {
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}