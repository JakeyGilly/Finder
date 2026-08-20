using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class IntegrationAccount {
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}