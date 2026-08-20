using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;

public class WelcomeScreen {
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("welcome_channels")]
    public WelcomeScreenChannel[] WelcomeChannels { get; set; }
}