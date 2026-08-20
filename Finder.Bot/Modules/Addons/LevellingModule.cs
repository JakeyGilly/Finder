using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Attributes;
using Finder.Db.UnitOfWork;

namespace Finder.Bot.Modules.Addons; 

[RequireAddon(Shared.Enum.Addons.Levelling)]
[Group("levelling", "Command For Managing Levelling")]
public class LevellingModule(IBotUnitOfWork botUnitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("level", "Get your current level", runMode: RunMode.Async)]
    public async Task LevelCommand() {
        var levels = await botUnitOfWork.Levelling.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id);
        if (levels == null) {
            botUnitOfWork.Levelling.AddItem(levels = new() {
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id,
            });
        }
        await botUnitOfWork.SaveChangesAsync();
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
        if (await botUnitOfWork.Addons.GetItemAsync(m => m.GuildId == guildId && m.Addon == Shared.Enum.Addons.Levelling && m.Enabled) == null) {
            return;
        }
        if (message.Author.IsBot) return;
        var levels = await botUnitOfWork.Levelling.GetItemAsync((m) => m.GuildId == guildId && m.UserId == userId);
        if (levels == null) {
            botUnitOfWork.Levelling.AddItem(levels = new() {
                GuildId = guildId,
                UserId = userId,
            });
        }
        var expToGet = (int)(50 * (levels.Level + 1) * Math.Sqrt(levels.Level + 1))/2;
        if (++levels.Exp > expToGet) {
            levels.Level++;
            levels.Exp = 0;
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
        }
        await botUnitOfWork.SaveChangesAsync();
    }
}