using Discord.Rest;
namespace Finder.Web.Models.DTO;

public class GuildDashboardDTO {
    public RestGuild? Guild { get; set; }
    public List<RestGuildUser> GuildMembers { get; set; } = new();
    public List<RestGuildChannel> GuildChannels { get; set; } = new();
}