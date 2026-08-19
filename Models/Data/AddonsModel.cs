using Finder.Bot.Db;
using Finder.Bot.Enums;
using Newtonsoft.Json;

namespace Finder.Bot.Models.Data;

public class AddonsModel: ICosmosItem {
    [JsonProperty(PropertyName = "id")]
    public required string Id { get; set; } // guild Id

    [JsonProperty(PropertyName = "addons")]
    public required Dictionary<Addons, bool> Addons { get; set; } = new();
}