using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;
public class Overwrite {
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("allow")]
    public string Allow { get; set; }
    [JsonProperty("deny")]
    public string Deny { get; set; }
}