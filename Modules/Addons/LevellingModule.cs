using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Attributes;
using Finder.Bot.Db.Models;
using Finder.Bot.Db.Repositories;

namespace Finder.Bot.Modules.Addons; 

[RequireAddon(Enums.Addons.Levelling)]
[Group("levelling", "Command For Managing Levelling")]
public class LevellingModule(IUnitOfWork unitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("level", "Get your current level", runMode: RunMode.Async)]
    public async Task LevelCommand() {
        var levels = await unitOfWork.Levelling.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id) ?? new LevellingModel {
            GuildId = Context.Guild.Id,
            UserId = Context.User.Id,
            Level = 0,
            Exp = 0
        };
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Level",
            Fields = [
                new() {
                    Name = "Level",
                    Value = levels.Level.ToString()
                },
                new() {
                    Name = "Exp",
                    Value = levels.Exp.ToString()
                }
            ],
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        }.Build());
    }
        
    public async Task OnMessageReceivedEvent(SocketMessage message) {
        var guildId = ((SocketGuildChannel)message.Channel).Guild.Id;
        var userId = message.Author.Id;
        if (!await unitOfWork.Addons.AddonEnabledInGuildAsync(guildId, Enums.Addons.Levelling)) return;
        if (message.Author.IsBot) return;
        var levels = await unitOfWork.Levelling.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id) ?? new LevellingModel {
            GuildId = Context.Guild.Id,
            UserId = Context.User.Id,
            Level = 0,
            Exp = 0
        };
        var expToGet = 50 * (int)Math.Pow(1.5, levels.Level + 1);
        if (++levels.Exp > expToGet)
        {
            levels.Level++;
            levels.Exp = 0;
            await unitOfWork.Levelling.UpsertItemAsync((m) => m.GuildId == guildId && m.UserId == userId, levels);
            await message.Channel.SendMessageAsync(embed: new EmbedBuilder {
                Title = $"Level Up {message.Author.Username}",
                Fields = [
                    new() {
                        Name = "You have leveled up to level",
                        Value = levels.Level + 1
                    }
                ],
                Footer = new EmbedFooterBuilder {
                    Text = "FinderBot"
                }
            }.Build());
        } else {
            await unitOfWork.Levelling.UpsertItemAsync((m) => m.GuildId == guildId && m.UserId == userId, levels);
        }
    }
}