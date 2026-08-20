using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Finder.Bot.Db.Models;
using Finder.Bot.Enums;
using Finder.Bot.Factories;
using Finder.Bot.Models;
using Finder.Db.UnitOfWork;

namespace Finder.Bot.Modules; 

[Group("moderation", "Moderation commands.")]
public class ModerationModule(IBotUnitOfWork botUnitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    private static readonly List<ModerationMessage> ModerationMessages = new();
    [SlashCommand("ban", "Bans a user from the server.", runMode: RunMode.Async)]
    public Task BanCommand(SocketGuildUser user, string reason = "No reason given.") 
        => RequestActionAsync(user, ModerationMessageType.Ban, "ban", reason);

    [SlashCommand("kick", "Kicks a user from the server.", runMode: RunMode.Async)]
    public Task KickCommand(SocketGuildUser user, string reason = "No reason given.") 
        => RequestActionAsync(user, ModerationMessageType.Kick, "kick", reason);

    [SlashCommand("warn", "Warns a user.", runMode: RunMode.Async)]
    public Task WarnCommand(SocketGuildUser user, string reason = "No reason given.") 
        => RequestActionAsync(user, ModerationMessageType.Warn, "warn", reason);

    [SlashCommand("mute", "Mutes a user.", runMode: RunMode.Async)]
    public Task MuteCommand(SocketGuildUser user, string reason = "No reason given.") 
        => RequestActionAsync(user, ModerationMessageType.Mute, "mute", reason);
    
    [SlashCommand("unmute", "Unmutes a user.", runMode: RunMode.Async)] 
    public Task UnmuteCommand(SocketGuildUser user) 
        => RequestActionAsync(user, ModerationMessageType.Unmute, "unmute");
    
    [SlashCommand("unban", "Unbans a user.", runMode: RunMode.Async)]
    public Task UnbanCommand(SocketGuildUser user) 
        => RequestActionAsync(user, ModerationMessageType.Unban, "unban");

    [SlashCommand("tempban", "Bans a user for a certain amount of time.", runMode: RunMode.Async)]
    public Task TempBanCommand(SocketGuildUser user, string time, string reason = "No reason given.") 
        => RequestActionAsync(user, ModerationMessageType.Tempban, "temp ban", reason, time);

    [SlashCommand("tempmute", "Mutes a user for a certain amount of time.", runMode: RunMode.Async)]
    public Task TempMuteCommand(SocketGuildUser user, string time, string reason = "No reason given.") 
        => RequestActionAsync(user, ModerationMessageType.Tempmute, "temp mute", reason, time);

    [SlashCommand("logs", "Displays the logs for a user.", runMode: RunMode.Async)]
    public async Task LogsCommand(IUser? user = null) {
        user ??= Context.User;
        var logs = await botUnitOfWork.UserLogs.GetItemAsync(m => m.GuildId == Context.Guild.Id && m.UserId == user.Id) ?? new UserLogsModel() {
            GuildId = Context.Guild.Id,
            UserId = user.Id
        };
        var muteRoleId = await botUnitOfWork.Settings.GetItemAsync(m => m.GuildId == Context.Guild.Id && m.Setting == "muteRoleId");
        var isMuted = muteRoleId != null && ((SocketGuildUser)user).Roles.Any(x => x.Id == ulong.Parse(muteRoleId.Value));
        await RespondAsync(embed: new ModerationEmbedFactory().BuildEmbed($"Logs for {user.Username}")
            .AddField("Warnings", logs.Warns, true)
            .AddField("Mutes", logs.Mutes, true)
            .AddField("Kicks", logs.Kicks, true)
            .AddField("Bans", logs.Bans, true)
            .AddField("Is Muted", isMuted ? "Yes" : "No", true)
            .Build());
    }

    public async Task OnReactionAddedEvent(Cacheable<IUserMessage, ulong> cacheMessage,
        Cacheable<IMessageChannel, ulong> cacheChannel, SocketReaction reaction)
    {
        if (reaction.User.Value.IsBot || reaction.Emote.Name != "✅") return;
        var modMsg =
            ModerationMessages.FirstOrDefault(m => m.MessageId == reaction.MessageId && m.SenderId == reaction.UserId);
        if (modMsg == null) return;

        var guild = ((SocketGuildChannel)reaction.Channel).Guild;
        var channel = (SocketTextChannel)guild.GetChannel(modMsg.ChannelId);
        var message = await channel.GetMessageAsync(modMsg.MessageId);
        var user = guild.GetUser(modMsg.UserId);

        var userLogs = await botUnitOfWork.UserLogs.GetItemAsync(m => m.GuildId == guild.Id && m.UserId == user.Id);
        if (userLogs == null)
        {
            botUnitOfWork.UserLogs.AddItem(userLogs = new()
            {
                GuildId = guild.Id,
                UserId = user.Id
            });
        }

        string actionPastTense = modMsg.Type + (modMsg.Type.ToString().EndsWith("e") ? "d" : "ed");
        await ExecuteModerationAction(modMsg, guild, user, channel, userLogs);
        await SendEmbeds(message, channel, user, guild, actionPastTense, modMsg);

        await message.RemoveAllReactionsAsync();
        ModerationMessages.Remove(modMsg);
        await botUnitOfWork.SaveChangesAsync();
    }

    private async Task RequestActionAsync(SocketGuildUser user, ModerationMessageType type, string actionName, string? reason = null, string? time = null) {
        DateTime? timeSpan = time != null ? DateTimeOffset.Now.Add(TimeSpan.Parse(time)).DateTime : null;
        
        var embed = new ModerationEmbedFactory().BuildEmbed($"Are you sure you want to {actionName} this user?", user, reason, timeSpan?.ToString());
        await RespondAsync(embed: embed.Build());
        
        var message = await GetOriginalResponseAsync();
        await message.AddReactionAsync(new Emoji("✅"));
        
        ModerationMessages.Add(new ModerationMessage {
            MessageId = message.Id,
            ChannelId = message.Channel.Id,
            GuildId = Context.Guild.Id,
            SenderId = Context.User.Id,
            UserId = user.Id,
            Reason = reason ?? "No reason given.",
            Type = type, Time = timeSpan
        });
    }
    
    private async Task SendEmbeds(IMessage message, SocketTextChannel channel, SocketGuildUser user, SocketGuild guild, string pastTenseAction, ModerationMessage modMsg) {
        await channel.ModifyMessageAsync(
            message.Id, m => {
                m.Embed = new ModerationEmbedFactory()
                    .BuildEmbed($"User {pastTenseAction}", user, modMsg.Reason, modMsg.Time?.ToString())
                    .Build();
            });
        try {
            await user.SendMessageAsync(embed: new ModerationEmbedFactory()
                .BuildEmbed($"You have been {pastTenseAction}", null, modMsg.Reason, modMsg.Time?.ToString())
                .AddField("Server", guild.Name)
                .WithColor(Color.Red)
                .WithThumbnailUrl(guild.IconUrl)
                .Build());
        } catch(HttpException) {
            // User has DMs disabled
        }
    }
    
    private async Task ExecuteModerationAction(ModerationMessage modMsg, SocketGuild guild, SocketGuildUser user, SocketTextChannel channel, UserLogsModel userLogs) {
        switch(modMsg.Type) {
            case ModerationMessageType.Ban:
            case ModerationMessageType.Tempban:
                await guild.AddBanAsync(user, reason: modMsg.Reason);
                userLogs.Bans++;
                if (modMsg.Type == ModerationMessageType.Tempban) {
                    userLogs.TempBan = modMsg.Time!.Value.ToUniversalTime();
                }
                break;
            case ModerationMessageType.Kick:
                await user.KickAsync(modMsg.Reason);
                userLogs.Kicks++;
                break;
            case ModerationMessageType.Warn:
                userLogs.Warns++;
                break;
            case ModerationMessageType.Mute:
            case ModerationMessageType.Tempmute:
                await ApplyMuteRole(guild, user, channel);
                userLogs.Mutes++; 
                if (modMsg.Type == ModerationMessageType.Tempmute) {
                    userLogs.TempMute = modMsg.Time!.Value.ToUniversalTime();
                }
                break;
            case ModerationMessageType.Unmute:
                await RemoveMuteRole(guild, user);
                break;
            case ModerationMessageType.Unban:
                await guild.RemoveBanAsync(user.Id);
                break;
        }
    }
    
    private async Task ApplyMuteRole(SocketGuild guild, SocketGuildUser user, SocketTextChannel channel) {
        var muteRoleSetting = await botUnitOfWork.Settings.GetItemAsync(m => m.GuildId == guild.Id && m.Setting == "muteRoleId");
        if (muteRoleSetting == null) {
            var newRole = await guild.CreateRoleAsync("Muted", new GuildPermissions(connect: true, readMessageHistory: true), Color.DarkGrey, false, true);
            botUnitOfWork.Settings.AddItem(muteRoleSetting = new() {
                GuildId = guild.Id,
                Setting = "muteRoleId",
                Value = newRole.Id.ToString()
            });
            await botUnitOfWork.SaveChangesAsync();
            foreach (var ch in guild.Channels) {
                await channel.AddPermissionOverwriteAsync(newRole,
                    OverwritePermissions
                        .DenyAll(channel)
                        .Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow));
            }
        }
        await user.AddRoleAsync(guild.GetRole(ulong.Parse(muteRoleSetting.Value)));
    }
    
    private async Task RemoveMuteRole(SocketGuild guild, SocketGuildUser user) {
        var muteRoleSetting = await botUnitOfWork.Settings.GetItemAsync(m => m.GuildId == guild.Id && m.Setting == "muteRoleId");
        if (muteRoleSetting != null) {
            await user.RemoveRoleAsync(guild.GetRole(ulong.Parse(muteRoleSetting.Value)));
        }
    }
}