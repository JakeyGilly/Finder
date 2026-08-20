using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Db.UnitOfWork;

namespace Finder.Bot.Modules; 

public class CountdownModule(IBotUnitOfWork botUnitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("countdown", "Countdown to a specific date or time", runMode: RunMode.Async)]
    public async Task CountdownCommand(long datetime, IMentionable? ping = null) {
        if (datetime < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) {
            await RespondAsync("Date or time is in the past", ephemeral: true);
            return;
        }
        if (datetime > DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds()) {
            await RespondAsync("The date or time is too far in the future", ephemeral: true);
            return;
        }
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Countdown",
            Fields = [
                new() {
                    Name = "Countdown ends in",
                    Value = $"<t:{datetime}:R>",
                }
            ],
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        }.Build());
        if (ping == null) {
            botUnitOfWork.Countdown.AddItem(new() {
                Id = Guid.NewGuid().ToString(),
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                UnixTime = datetime
            });
            await botUnitOfWork.SaveChangesAsync();
            return;
        }
        switch (ping) {
            case SocketRole role:
                botUnitOfWork.Countdown.AddItem(new() {
                    Id = Guid.NewGuid().ToString(),
                    GuildId = Context.Guild.Id,
                    ChannelId = Context.Channel.Id,
                    UnixTime = datetime,
                    PingRoleId = role.Id
                });
                break;
            case SocketGuildUser user:
                botUnitOfWork.Countdown.AddItem(new() {
                    Id = Guid.NewGuid().ToString(),
                    GuildId = Context.Guild.Id,
                    ChannelId = Context.Channel.Id,
                    UnixTime = datetime,
                    PingUserId = user.Id
                });
                break;
            default:
                await RespondAsync("Invalid mention", ephemeral: true);
                break;
        }
        await botUnitOfWork.SaveChangesAsync();
    }
}