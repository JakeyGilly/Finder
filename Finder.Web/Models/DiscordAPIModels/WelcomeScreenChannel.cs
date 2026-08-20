using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class WelcomeScreenChannel {
    [JsonProperty("channel_id")]
    public string ChannelId { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("emoji_id")]
    public string EmojiId { get; set; }
    [JsonProperty("emoji_name")]
    public string EmojiName { get; set; }
}