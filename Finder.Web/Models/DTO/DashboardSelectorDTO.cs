using Discord;
using Discord.Rest;
namespace Finder.Web.Models.DTO;

public class DashboardSelectorDTO {
    public List<RestUserGuild>? BotGuilds { get; set; } = new();
    public List<RestUserGuild>? UserGuilds { get; set; } = new();
    public RestUser? UserProfile { get; set; }
}