using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class Sticker {
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("pack_id")]
    public ulong? PackId { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string? Description { get; set; }
    [JsonProperty("tags")]
    public string Tags { get; set; }
    [JsonProperty("asset")]
    public string? Asset { get; set; }
    [JsonProperty("type")]
    public StickerType Type { get; set; }
    [JsonProperty("format_type")]
    public StickerFormat FormatType { get; set; }
    [JsonProperty("available")]
    public bool? Available { get; set; }
    [JsonProperty("guild_id")]
    public ulong? GuildId { get; set; }
    [JsonProperty("user")]
    public User? User { get; set; }
    [JsonProperty("sort_value")]
    public int? SortValue { get; set; }
}
