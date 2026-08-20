using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using Newtonsoft.Json;

namespace Finder.Bot.Handlers; 

public class InteractionHandler {
    private readonly InteractionService commands;
    private readonly DiscordShardedClient client;
    private readonly IServiceProvider services;
    public InteractionHandler(InteractionService _commands, DiscordShardedClient _client, IServiceProvider _services) {
        commands = _commands;
        client = _client;
        services = _services;
    }

    public async Task InitializeAsync() {
        await commands.AddModulesAsync(typeof(InteractionHandler).Assembly, services);
        client.InteractionCreated += InteractionCreated;
        client.ButtonExecuted += ButtonExecuted;
        client.ShardReady += ShardReady;
        commands.SlashCommandExecuted += CommandsSlashCommandExecuted;
        commands.AutocompleteHandlerExecuted += CommandsAutocompleteHandlerExecuted;
    }

    private Task CommandsAutocompleteHandlerExecuted(IAutocompleteHandler arg1, Discord.IInteractionContext arg2, IResult arg3) {
        return Task.CompletedTask;
    }

    private Task CommandsSlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3) {
        return Task.CompletedTask;
    }

    private async Task ButtonExecuted(SocketMessageComponent arg) {
        await commands.ExecuteCommandAsync(new ShardedInteractionContext(client, arg), services);
    }

    private async Task ShardReady(DiscordSocketClient arg) {
        await RegisterCommands();
    }

    private async Task InteractionCreated(SocketInteraction arg) {
        _ = Task.Run(async () =>
        {
            var context = new ShardedInteractionContext(client, arg);
            await commands.ExecuteCommandAsync(context, services);
        });
        await Task.CompletedTask;
    }

    private async Task RegisterCommands() {
        foreach (var guild in client.Guilds) {
            await commands.RegisterCommandsToGuildAsync(guild.Id);
        }
    }
}