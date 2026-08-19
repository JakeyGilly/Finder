using System.Configuration;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Db;
using Finder.Bot.Db.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Finder.Bot.Handlers;
using Finder.Bot.Modules.Addons;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Finder.Bot;

class Program {
    static void Main() => RunAsync().GetAwaiter().GetResult();
    static async Task RunAsync() {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        await using ServiceProvider services = ConfigureServices(configuration.GetSection("CosmosDb"));
        DiscordShardedClient client = services.GetRequiredService<DiscordShardedClient>();
        InteractionService commands = services.GetRequiredService<InteractionService>();
        InteractionHandler handler = services.GetRequiredService<InteractionHandler>();
        await handler.InitializeAsync();
        client.Log += LoggingService.LogAsync;
        commands.Log += LoggingService.LogAsync;
        // UnBanMuteTimer.StartTimer(client, services.GetRequiredService<IUnitOfWork>());
        // CountdownTimer.StartTimer(client, services.GetRequiredService<IUnitOfWork>());
        client.ReactionAdded += TicTacToeModule.OnReactionAddedEvent;
        // client.ReactionAdded += new ModerationModule(services.GetRequiredService<IUnitOfWork>()).OnReactionAddedEvent;
        // client.ButtonExecuted += new PollModule(services.GetRequiredService<IUnitOfWork>()).OnButtonExecutedEvent;
        // client.ButtonExecuted += new TicketingModule.TicketsModule(services.GetRequiredService<IUnitOfWork>()).OnButtonExecutedEvent;
        // client.MessageReceived += new LevelingModule(services.GetRequiredService<IUnitOfWork>()).OnMessageReceivedEvent;
        await client.LoginAsync(TokenType.Bot, configuration.GetSection("BotToken").Value);
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }
    
    private static readonly DiscordSocketConfig discordConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildEmojis
    };
    
    private static ServiceProvider ConfigureServices(IConfigurationSection configurationSection) {
        return new ServiceCollection()
            .AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync(configurationSection).GetAwaiter().GetResult())
            .AddSingleton<IUnitOfWork, UnitOfWork>()
            .AddSingleton<DiscordShardedClient>(x => new DiscordShardedClient(discordConfig))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>()))
            .AddSingleton<InteractionHandler>()
            .BuildServiceProvider();
    }
    
    private static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection) {
        string databaseName = configurationSection.GetSection("DatabaseName").Value;
        string containerName = configurationSection.GetSection("ContainerName").Value;
        string account = configurationSection.GetSection("DbUri").Value;
        string key = configurationSection.GetSection("PrimaryKey").Value;
        CosmosClient client = new CosmosClient(account, key);
        CosmosDbService cosmosDbService = new CosmosDbService(client, databaseName, containerName);
        DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
        await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
        return cosmosDbService;
    }
}
