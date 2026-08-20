using Newtonsoft.Json;
namespace Finder.Web.Models.DiscordAPIModels;
public class ThreadMember {
    [JsonProperty("id")]
    public ulong? Id { get; set; }
    [JsonProperty("user_id")]
    public ulong? UserId { get; set; }
    [JsonProperty("join_timestamp")]
    public DateTime JoinTimestamp { get; set; }
    [JsonProperty("flags")]
    public int Flags { get; set; }
}