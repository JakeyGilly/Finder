// using Discord;
// using Discord.WebSocket;
// using Finder.Bot.Repositories;
// using System.Timers;
// namespace Finder.Bot.Modules.Helpers;
//
// public static class UnBanMuteTimer {
//     private static System.Timers.Timer _messageTimer;
//     private static DiscordShardedClient _client;
//     private static IUnitOfWork _unitOfWork;
//     public static void StartTimer(DiscordShardedClient client, IUnitOfWork unitOfWork) {
//         _client = client;
//         _unitOfWork = unitOfWork;
//         _messageTimer = new System.Timers.Timer(5000);
//         _messageTimer.Elapsed += OnTimerElapsed;
//         _messageTimer.AutoReset = true;
//         _messageTimer.Enabled = true;
//     }
//
//     private static async void OnTimerElapsed(object source, ElapsedEventArgs e) {
//         foreach (var c in await _unitOfWork.UserLogs.GetAllAsync()) {
//             var guild = _client.GetGuild((ulong)c.GuildId);
//             SocketGuildUser user;
//             if (c.TempBan != null && c.TempBan < DateTime.UtcNow) {
//                 await guild.RemoveBanAsync((ulong)c.UserId);
//                 try {
//                     user = guild.GetUser((ulong)c.UserId);
//                     await user.SendMessageAsync(embed: new EmbedBuilder {
//                         Title = "You have been unbanned",
//                         Fields = new List<EmbedFieldBuilder> {
//                             new()
//                             {
//                                 Name = "Server",
//                                 Value = $"{guild.Name}",
//                                 IsInline = false
//                             }
//                         },
//                         Footer = new EmbedFooterBuilder {
//                             Text = "FinderBot"
//                         },
//                         ThumbnailUrl = guild.IconUrl
//                     }.Build());
//                 } catch (Exception) {
//                     // ignored
//                 }
//                 await _unitOfWork.UserLogs.RemoveTempbanTime((ulong)c.GuildId, (ulong)c.UserId);
//                 await _unitOfWork.SaveChangesAsync();
//                 continue;
//             }
//             if (c.TempMute == null || !(c.TempMute < DateTime.UtcNow)) continue;
//             try {
//                 user = guild.GetUser((ulong)c.UserId);
//                 await user.SendMessageAsync(embed: new EmbedBuilder {
//                     Title = "You have been Unmuted",
//                     Fields = new List<EmbedFieldBuilder> {
//                         new()
//                         {
//                             Name = "Server",
//                             Value = $"{guild.Name}",
//                             IsInline = false
//                         }
//                     },
//                     Footer = new EmbedFooterBuilder {
//                         Text = "FinderBot"
//                     },
//                     ThumbnailUrl = guild.IconUrl
//                 }.Build());
//                 var muteRole = guild.GetRole(Convert.ToUInt64(await _unitOfWork.Settings.GetSettingAsync(guild.Id, "muteRoleId")));
//                 await user.RemoveRoleAsync(muteRole);
//             } catch (Exception) {
//                 // ignored
//             }
//             await _unitOfWork.UserLogs.RemoveTempmuteTime((ulong)c.GuildId, (ulong)c.UserId);
//             await _unitOfWork.SaveChangesAsync();
//         }
//     }
// }