using Discord;
using Discord.WebSocket;

namespace Finder.Bot.Factories;

public class ModerationEmbedFactory {
    public EmbedBuilder BuildEmbed(string title, IUser? user = null, string? reason = null, string? time = null) {
        var embed = new EmbedBuilder { Title = title, Footer = new EmbedFooterBuilder { Text = "FinderBot" } };
        if (user != null) embed.AddField("User", $"{user.Mention} ({user.Username})", false);
        if (reason != null && reason != "No reason given.") embed.AddField("For reason", reason, false);
        if (time != null) embed.AddField("Until/For time", time, false);
        return embed;
    }
}