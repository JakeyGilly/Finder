// using Discord;
// using Discord.Interactions;
// using Discord.WebSocket;
// using Finder.Bot.Repositories;
// using Pathoschild.NaturalTimeParser.Parser;
// namespace Finder.Bot.Modules; 
//
// public class CountdownModule : InteractionModuleBase<ShardedInteractionContext> {
//     private readonly IUnitOfWork _unitOfWork;
//     public CountdownModule(IUnitOfWork unitOfWork) {
//         _unitOfWork = unitOfWork;
//     }
//     [SlashCommand("countdown", "Countdown to a specific date or time", runMode: RunMode.Async)]
//     public async Task CountdownCommand(int datetime, IMentionable? ping = null) {
//         long date;
//         try {
//             // date = DateTimeOffset.Parse(datetime).ToUnixTimeSeconds();
//             date = datetime;
//         } catch (TimeParseFormatException) {
//             await RespondAsync("Invalid date or time");
//             return;
//         }
//         if (date < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) {
//             await RespondAsync("Date or time is in the past");
//             return;
//         }
//         if (date > DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds()) {
//             await RespondAsync("The date or time is too far in the future");
//             return;
//         }
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = "Countdown",
//             Fields = new List<EmbedFieldBuilder> {
//                 new() {
//                     Name = "Countdown ends in",
//                     Value = $"<t:{date}:R>",
//                 },
//             },
//             Footer = new EmbedFooterBuilder {
//                 Text = "FinderBot"
//             }
//         }.Build());
//         if (ping != null) {
//             switch(ping) {
//                 case SocketRole role:
//                     await _unitOfWork.Countdown.AddCountdownAsync(Context.Channel.Id, Context.Guild.Id, date, null, role.Id);
//                     break;
//                 case SocketGuildUser user:
//                     await _unitOfWork.Countdown.AddCountdownAsync(Context.Channel.Id, Context.Guild.Id, date, user.Id);
//                     break;
//                 default:
//                     await RespondAsync("Invalid mention");
//                     break;
//             }
//         } else {
//             await _unitOfWork.Countdown.AddCountdownAsync(Context.Channel.Id, Context.Guild.Id, date);
//         }
//         await _unitOfWork.SaveChangesAsync();
//     }
// }