using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class IntegrationApplication {
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("icon")]
    public string? Icon { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("bot")]
    public User? Bot { get; set; }
}