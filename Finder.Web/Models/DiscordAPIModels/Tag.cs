using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;
public class Tag {
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("moderated")]
    public bool Moderated { get; set; }
    [JsonProperty("emoji_id")]
    public ulong? EmojiId { get; set; }
    [JsonProperty("emoji_name")]
    public string? EmojiName { get; set; }
}