using Discord;
using Discord.Interactions;

namespace Finder.Bot.Modules;

public class BasicModule : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("echo", "Repeat the input")]
    public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
        => await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));

    [SlashCommand("ping", "Pings the bot and returns its latency.")]
    public async Task GreetUserAsync()
        => await RespondAsync(text: $":ping_pong: It took me {Context.Client.Latency}ms to respond to you!", ephemeral: true);
    
    [SlashCommand("greet", "Greet a user")]
    public async Task GreetUserAsync(IUser user)
        => await RespondAsync(text: $":wave: {Context.User} said hi to you, <@{user.Id}>!");

    [SlashCommand("think", "Think about something")]
    public async Task ThinkAsync()
        => await DeferAsync();
}