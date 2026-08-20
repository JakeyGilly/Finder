using Finder.Web.Models.DiscordAPIModels;
namespace Finder.Web.Models.DTO;

public class GuildDashboardDTO {
    public Guild? Guild { get; set; }
    public List<GuildMember>? GuildMembers { get; set; } = new();
    public List<GuildChannel>? GuildChannels { get; set; } = new();
}