using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class Connections {
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("revoked")]
    public bool? Revoked { get; set; }
    
}