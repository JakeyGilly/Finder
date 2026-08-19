using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Db.Models;
using Finder.Bot.Db.Repositories;

namespace Finder.Bot.Modules; 

public class CountdownModule(IUnitOfWork unitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("countdown", "Countdown to a specific date or time", runMode: RunMode.Async)]
    public async Task CountdownCommand(long datetime, IMentionable? ping = null) {
        if (datetime < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) {
            await RespondAsync("Date or time is in the past");
            return;
        }
        if (datetime > DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds()) {
            await RespondAsync("The date or time is too far in the future");
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
            await unitOfWork.Countdown.AddItemAsync(new CountdownModel() {
                Id = Guid.NewGuid().ToString(),
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                UnixTime = datetime
            });
            return;
        }
        switch (ping) {
            case SocketRole role:
                await unitOfWork.Countdown.AddItemAsync(new CountdownModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    GuildId = Context.Guild.Id,
                    ChannelId = Context.Channel.Id,
                    UnixTime = datetime,
                    PingRoleId = role.Id
                });
                break;
            case SocketGuildUser user:
                await unitOfWork.Countdown.AddItemAsync(new CountdownModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    GuildId = Context.Guild.Id,
                    ChannelId = Context.Channel.Id,
                    UnixTime = datetime,
                    PingUserId = user.Id
                });
                break;
            default:
                await RespondAsync("Invalid mention");
                break;
        }
    }
}