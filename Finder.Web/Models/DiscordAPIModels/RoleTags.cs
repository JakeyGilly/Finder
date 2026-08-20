using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class RoleTags {
    [JsonProperty("bot_id")]
    public ulong? BotId { get; set; }
    [JsonProperty("integration_id")]
    public ulong? IntegrationId { get; set; }
    [JsonProperty("premium_subscriber")]
    public bool? IsPremiumSubscriber { get; set; }
}
