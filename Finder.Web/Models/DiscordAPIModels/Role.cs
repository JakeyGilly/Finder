using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class Role {
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("color")]
    public int Color { get; set; }
    [JsonProperty("hoist")]
    public bool Hoist { get; set; }
    [JsonProperty("icon")]
    public string? Icon { get; set; }
    [JsonProperty("unicode_emoji")]
    public string? Emoji { get; set; }
    [JsonProperty("position")]
    public int Position { get; set; }
    [JsonProperty("permissions")]
    public string Permissions { get; set; }
    [JsonProperty("managed")]
    public bool Managed { get; set; }
    [JsonProperty("mentionable")]
    public bool Mentionable { get; set; }
    [JsonProperty("tags")]
    public RoleTags? Tags { get; set; }
}
