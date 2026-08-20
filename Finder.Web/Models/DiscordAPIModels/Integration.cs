using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class Integration {
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("enabled")]
    public bool? Enabled { get; set; }
    [JsonProperty("syncing")]
    public bool? Syncing { get; set; }
    [JsonProperty("role_id")]
    public ulong? RoleId { get; set; }
    [JsonProperty("enable_emoticons")]
    public bool? EnableEmoticons { get; set; }
    [JsonProperty("expire_behavior")]
    public IntegrationExpireBehaviors? ExpireBehavior { get; set; }
    [JsonProperty("expire_grace_period")]
    public int? ExpireGracePeriod { get; set; }
    [JsonProperty("user")]
    public User? User { get; set; }
    [JsonProperty("account")]
    public IntegrationAccount? Account { get; set; }
    [JsonProperty("synced_at")]
    public DateTime? SyncedAt { get; set; }
    [JsonProperty("subscriber_count")]
    public int? SubscriberCount { get; set; }
    [JsonProperty("revoked")]
    public bool? Revoked { get; set; }
    [JsonProperty("application")]
    public IntegrationApplication Application { get; set; }
}