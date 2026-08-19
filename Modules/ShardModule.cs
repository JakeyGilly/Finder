using Discord;
using Discord.Interactions;
using System.Diagnostics;
using Finder.Bot.Handlers;

namespace Finder.Bot.Modules; 

[Group("shard", "Command for shard management")]
public class ShardModule : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("info", "Get information about the shard", runMode: RunMode.Async)]
    public async Task Info() {
        var ramMb = (Process.GetCurrentProcess().WorkingSet64 / 1e+6).ToString("F2");
        var shard = Context.Client.GetShardFor(Context.Guild);
        var shardCount = Context.Client.Shards.Count;
        var guildCount = shard.Guilds.Count;
        var userCount = shard.Guilds.Sum(guild => guild.MemberCount);
        var ping = shard.Latency;
        var uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
        var emoji = ping switch {
            < 200 => "🟢",
            < 500 => "🟠",
            _ => "🔴"
        };

        await RespondAsync(embed: new EmbedBuilder {
            Title = "Shard info",
            Description = $"{emoji} Shard {shard.ShardId+1}/{shardCount}",
            Fields = new List<EmbedFieldBuilder> {
                new()
                {
                    Name = "Guilds",
                    Value = guildCount.ToString(),
                    IsInline = true
                },
                new()
                {
                    Name = "Users",
                    Value = userCount.ToString(),
                    IsInline = true
                },
                new()
                {
                    Name = "RAM",
                    Value = $"{ramMb} MB",
                    IsInline = true
                },
                new()
                {
                    Name = "Ping",
                    Value = $"{ping}ms",
                    IsInline = true
                },
                new()
                {
                    Name = "Uptime",
                    Value = uptime.ToString(@"dd\.hh\:mm\:ss"),
                    IsInline = true
                }
            }
        }.Build());
    }
}