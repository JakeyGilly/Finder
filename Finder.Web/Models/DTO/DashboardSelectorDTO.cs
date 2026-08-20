using Finder.Web.Models.DiscordAPIModels;
namespace Finder.Web.Models.DTO;

public class DashboardSelectorDTO {
    public List<Guild>? BotGuilds { get; set; } = new();
    public List<Guild>? UserGuilds { get; set; } = new();
    public User? UserProfile { get; set; } = new();
}