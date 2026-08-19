using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Db;
using Finder.Bot.Db.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Finder.Bot.Handlers;
using Finder.Bot.Modules.Addons;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Finder.Bot;

class Program {
    static async Task Main() {
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
        client.ButtonExecuted += new TicketingModule(services.GetRequiredService<IUnitOfWork>()).OnButtonExecutedEvent;
        client.MessageReceived += new LevellingModule(services.GetRequiredService<IUnitOfWork>()).OnMessageReceivedEvent;
        await client.LoginAsync(TokenType.Bot, configuration.GetSection("BotToken").Value);
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }
    
    private static readonly DiscordSocketConfig discordConfig = new() {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildEmojis
    };
    
    private static ServiceProvider ConfigureServices(IConfigurationSection configurationSection) {
        string dbName = configurationSection.GetSection("DatabaseName").Value!;
        return new ServiceCollection()
            .AddDbContext<BotDbContext>(options => options.UseNpgsql(configurationSection.GetConnectionString("PostgreSQL")))
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddSingleton<DiscordShardedClient>(x => new DiscordShardedClient(discordConfig))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>()))
            .AddSingleton<InteractionHandler>()
            .BuildServiceProvider();
    }
}
