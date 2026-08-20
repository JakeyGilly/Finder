using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;
public class DefaultReaction {
    [JsonProperty("emoji_id")]
    public ulong? EmojiId { get; set; }
    [JsonProperty("emoji_name")]
    public string? EmojiName { get; set; }
}