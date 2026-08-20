using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class User {
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("username")]
    public string Username { get; set; }
    [JsonProperty("discriminator")]
    public string Discriminator { get; set; }
    [JsonProperty("avatar")]
    public string? Avatar { get; set; }
    [JsonProperty("bot")]
    public bool? Bot { get; set; }
    [JsonProperty("system")]
    public bool? System { get; set; }
    [JsonProperty("mfa_enabled")]
    public bool? MfaEnabled { get; set; }
    [JsonProperty("banner")]
    public string? Banner { get; set; }
    [JsonProperty("accent_color")]
    public int? AccentColor { get; set; }
    [JsonProperty("locale")]
    public string? Locale { get; set; }
    [JsonProperty("verified")]
    public bool? Verified { get; set; }
    [JsonProperty("email")]
    public string? Email { get; set; }
    [JsonProperty("flags")]
    public UserFlags Flags { get; set; }
    [JsonProperty("premium_type")]
    public PremiumType PremiumType { get; set; }
    [JsonProperty("public_flags")]
    public UserFlags PublicFlags { get; set; }
}