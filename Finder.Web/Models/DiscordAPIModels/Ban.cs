using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class Ban {
    [JsonProperty("reason")]
    public string? Reason { get; set; }
    [JsonProperty("user")]
    public User User { get; set; }
}