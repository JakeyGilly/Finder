using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class Guild {
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("icon")]
    public string? Icon { get; set; }
    [JsonProperty("icon_hash")]
    public string? IconHash { get; set; }
    [JsonProperty("splash")]
    public string? Splash { get; set; }
    [JsonProperty("discovery_splash")]
    public string? DiscoverySplash { get; set; }
    [JsonProperty("owner")]
    public bool? Owner { get; set; }
    [JsonProperty("owner_id")]
    public ulong OwnerId { get; set; }
    [JsonProperty("permissions")]
    public string? Permissions { get; set; }
    [JsonProperty("region")]
    public string? Region { get; set; }
    [JsonProperty("afk_channel_id")]
    public ulong? AFKChannelId { get; set; }
    [JsonProperty("afk_timeout")]
    public int AFKTimeout { get; set; }
    [JsonProperty("widget_enabled")]
    public bool? WidgetEnabled { get; set; }
    [JsonProperty("widget_channel_id")]
    public ulong? WidgetChannelId { get; set; }
    [JsonProperty("verification_level")]
    public VerificationLevel VerificationLevel { get; set; }
    [JsonProperty("default_message_notifications")]
    public DefaultMessageNotificationLevel DefaultMessageNotifications { get; set; }
    [JsonProperty("explicit_content_filter")]
    public ExplicitContentFilterLevel ExplicitContentFilter { get; set; }
    [JsonProperty("roles")]
    public List<Role> Roles { get; set; }
    [JsonProperty("emojis")]
    public List<Emoji> Emojis { get; set; }
    [JsonProperty("features")]
    public List<GuildFeatures> Features { get; set; }
    [JsonProperty("mfa_level")]
    public MFALevel MfaLevel { get; set; }
    [JsonProperty("application_id")]
    public ulong? ApplicationId { get; set; }
    [JsonProperty("system_channel_id")]
    public ulong? SystemChannelId { get; set; }
    [JsonProperty("system_channel_flags")]
    public SystemChannelFlags SystemChannelFlags { get; set; }
    [JsonProperty("rules_channel_id")]
    public ulong? RulesChannelId { get; set; }
    [JsonProperty("max_presences")]
    public int? MaxPresences { get; set; }
    [JsonProperty("max_members")]
    public int? MaxMembers { get; set; }
    [JsonProperty("vanity_url_code")]
    public string? VanityURLCode { get; set; }
    [JsonProperty("description")]
    public string? Description { get; set; }
    [JsonProperty("banner")]
    public string? Banner { get; set; }
    [JsonProperty("premium_tier")]
    public PremiumTier PremiumTier { get; set; }
    [JsonProperty("premium_subscription_count")]
    public int? PremiumSubscriptionCount { get; set; }
    [JsonProperty("preferred_locale")]
    public string PreferredLocale { get; set; }
    [JsonProperty("public_updates_channel_id")]
    public ulong? PublicUpdatesChannelId { get; set; }
    [JsonProperty("max_video_channel_users")]
    public int? MaxVideoChannelUsers { get; set; }
    [JsonProperty("approximate_member_count")]
    public int? ApproximateMemberCount { get; set; }
    [JsonProperty("approximate_presence_count")]
    public int? ApproximatePresenceCount { get; set; }
    [JsonProperty("welcome_screen")]
    public WelcomeScreen? WelcomeScreen { get; set; }
    [JsonProperty("nsfw_level")]
    public int NsfwLevel { get; set; }
    [JsonProperty("stickers")]
    public List<Sticker> Stickers { get; set; }
    [JsonProperty("premium_progress_bar_enabled")]
    public bool? IsBoostProgressBarEnabled { get; set; }
}
