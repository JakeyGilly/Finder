using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Attributes;
using Finder.Bot.Db.Repositories;

namespace Finder.Bot.Modules.Addons; 

[RequireAddon(Enums.Addons.Levelling)]
[Group("levelling", "Command For Managing Levelling")]
public class LevellingModule(IUnitOfWork unitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("level", "Get your current level", runMode: RunMode.Async)]
    public async Task LevelCommand() {
        var levels = await unitOfWork.Levelling.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id);
        if (levels == null) {
            unitOfWork.Levelling.AddItem(levels = new() {
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id,
            });
        }
        await unitOfWork.SaveChangesAsync();
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
        var levels = await unitOfWork.Levelling.GetItemAsync((m) => m.GuildId == guildId && m.UserId == userId);
        if (levels == null) {
            unitOfWork.Levelling.AddItem(levels = new() {
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
        await unitOfWork.SaveChangesAsync();
    }
}