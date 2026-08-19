using Newtonsoft.Json;

namespace Finder.Bot.Models.Data.Bot;

public class AddonsModel {
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    [JsonProperty(PropertyName = "addons")]
    public Dictionary<string, bool> Addons { get; set; }
}