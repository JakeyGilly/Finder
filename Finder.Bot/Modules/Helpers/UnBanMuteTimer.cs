using Discord;
using Discord.WebSocket;
using System.Timers;
using Finder.Bot.Factories;
using Finder.Db.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace Finder.Bot.Modules.Helpers;

public class UnBanMuteTimer(DiscordShardedClient client, IServiceProvider services) {
    private System.Timers.Timer _messageTimer;
    public void StartTimer() {
        _messageTimer = new System.Timers.Timer(5000) {
            AutoReset = false,
            Enabled = true
        };
        _messageTimer.Elapsed += async (s, e) => await OnTimerElapsed(s, e);
        _messageTimer.Start();
    }

    private async Task OnTimerElapsed(object? source, ElapsedEventArgs e) {
        try {
            using var scope = services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IBotUnitOfWork>();
            var now = DateTime.UtcNow;
            var punishments = await unitOfWork.UserLogs.GetItemsAsync(c =>
                (c.TempBan != null && c.TempBan < now) ||
                (c.TempMute != null && c.TempMute < now));
            if (!punishments.Any()) return;
            foreach (var log in punishments) {
                var guild = client.GetGuild(log.GuildId);
                if (guild == null) {
                    unitOfWork.UserLogs.DeleteItem(log);
                    continue;
                }
                // user isnt in guild
                var globalUser = client.GetUser(log.UserId) ?? (IUser)await client.Rest.GetUserAsync(log.UserId);
                if (log.TempBan != null && log.TempBan < now) {
                    await guild.RemoveBanAsync(log.UserId);
                    try {
                        globalUser = guild.GetUser(log.UserId);
                        await globalUser.SendMessageAsync(embed: new ModerationEmbedFactory()
                            .BuildEmbed("You have been unbanned", globalUser)
                            .AddField("Server", guild.Name)
                            .WithThumbnailUrl(guild.IconUrl)
                            .Build());
                    } catch (Exception) {
                        // ignored
                    }
                    log.TempBan = null;
                    continue;
                }
                if (log.TempMute == null || !(log.TempMute < now)) continue;
                try {
                    globalUser = guild.GetUser(log.UserId);
                    await globalUser.SendMessageAsync(embed: new ModerationEmbedFactory()
                        .BuildEmbed("You have been unmuted", globalUser)
                        .AddField("Server", guild.Name)
                        .WithThumbnailUrl(guild.IconUrl)
                        .Build());
                    var muteRoleSetting = await unitOfWork.Settings.GetItemAsync(m => m.GuildId == guild.Id && m.Setting == "muteRoleId");
                    if (muteRoleSetting != null) {
                        var guildUser = guild.GetUser(log.UserId);
                        if (guildUser != null) {
                            var muteRole = guild.GetRole(ulong.Parse(muteRoleSetting.Value));
                            if (muteRole != null) {
                                await guildUser.RemoveRoleAsync(muteRole);
                            }
                        }
                    }
                } catch (Exception) {
                    // ignored
                }
                log.TempMute = null;
            }
            await unitOfWork.SaveChangesAsync();
        } catch (Exception ex) {
            // A top-level catch prevents async void from crashing the process
            Console.WriteLine($"[Timer Error] {ex.Message}");
        } finally {
            _messageTimer.Start();
        }
    }
}