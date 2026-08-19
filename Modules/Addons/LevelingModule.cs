// using Discord;
// using Discord.Interactions;
// using Discord.WebSocket;
// using Finder.Bot.Models.Data.Bot;
// using Finder.Bot.Repositories;
//
// namespace Finder.Bot.Modules.Addons; 
//
// [Group("leveling", "Command For Managing Leveling")]
// public class LevelingModule : InteractionModuleBase<ShardedInteractionContext> {
//     private readonly IUnitOfWork _unitOfWork;
//     public LevelingModule(IUnitOfWork unitOfWork) {
//         _unitOfWork = unitOfWork;
//     }
//
//     [SlashCommand("level", "Get your current level", runMode: RunMode.Async)]
//     public async Task LevelCommand() {
//         if (!await _unitOfWork.Addons.AddonEnabled(Context.Guild.Id, "Leveling")) {
//             await RespondAsync(embed: new EmbedBuilder {
//                 Title = "Leveling",
//                 Description = "This addon is disabled on this server.",
//                 Fields = new List<EmbedFieldBuilder> {
//                     new()
//                     {
//                         Name = "Enable",
//                         Value = "Use `/addons install Leveling` to enable this addon."
//                     }
//                 }
//             }.Build());
//             return;
//         }
//         var levels = await _unitOfWork.Leveling.FindAsync(Context.Guild.Id, Context.User.Id) ?? new LevelingModel {
//             GuildId = (long)Context.Guild.Id,
//             UserId = (long)Context.User.Id,
//             Level = 0,
//             Exp = 0
//         };
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = "Level",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = "Level",
//                     Value = levels.Level.ToString()
//                 },
//                 new()
//                 {
//                     Name = "Exp",
//                     Value = levels.Exp.ToString()
//                 }
//             },
//             Footer = new EmbedFooterBuilder {
//                 Text = "FinderBot"
//             }
//         }.Build());
//     }
//         
//     public async Task OnMessageReceivedEvent(SocketMessage message) {
//         if (!await _unitOfWork.Addons.AddonEnabled(Context.Guild.Id, "Leveling")) return;
//         if (message.Author.IsBot) return;
//         var levels = await _unitOfWork.Leveling.FindAsync(((SocketGuildChannel)message.Channel).Guild.Id, message.Author.Id) ?? new LevelingModel {
//             GuildId = (long)((SocketGuildChannel)message.Channel).Guild.Id,
//             UserId = (long)message.Author.Id,
//             Level = 0,
//             Exp = 0
//         };
//         var expToGet = 50 * (int)Math.Pow(1.5, levels.Level + 1);
//         if (++levels.Exp > expToGet) {
//             await _unitOfWork.Leveling.AddLevelingAsync(((SocketGuildChannel)message.Channel).Guild.Id, message.Author.Id, levels.Level, 0);
//             await message.Channel.SendMessageAsync(embed: new EmbedBuilder {
//                 Title = $"Level Up {message.Author.Username}",
//                 Fields = new List<EmbedFieldBuilder> {
//                     new()
//                     {
//                         Name = "You have leveled up to level",
//                         Value = levels.Level + 1
//                     }
//                 },
//                 Footer = new EmbedFooterBuilder {
//                     Text = "FinderBot"
//                 }
//             }.Build());
//         } else {
//             await _unitOfWork.Leveling.AddLevelingAsync(((SocketGuildChannel)message.Channel).Guild.Id, message.Author.Id, levels.Level, levels.Exp);
//         }
//     }
// }