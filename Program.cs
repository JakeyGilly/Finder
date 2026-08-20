using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Db;
using Finder.Bot.Db.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Finder.Bot.Handlers;
using Finder.Bot.Modules;
using Finder.Bot.Modules.Addons;
using Finder.Bot.Modules.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Finder.Bot;

class Program {
    static async Task Main() {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        await using ServiceProvider services = ConfigureServices(configuration);
        DiscordShardedClient client = services.GetRequiredService<DiscordShardedClient>();
        InteractionService commands = services.GetRequiredService<InteractionService>();
        InteractionHandler handler = services.GetRequiredService<InteractionHandler>();
        await handler.InitializeAsync();
        client.Log += LoggingService.LogAsync;
        commands.Log += LoggingService.LogAsync;
        new UnBanMuteTimer(client, services).StartTimer();
        new CountdownTimer(client, services).StartTimer();
        client.ReactionAdded += TicTacToeModule.OnReactionAddedEvent;
        client.ReactionAdded += new ModerationModule(services.GetRequiredService<IUnitOfWork>()).OnReactionAddedEvent;
        client.ButtonExecuted += new PollModule(services.GetRequiredService<IUnitOfWork>()).OnButtonExecutedEvent;
        client.ButtonExecuted += new TicketingModule(services.GetRequiredService<IUnitOfWork>()).OnButtonExecutedEvent;
        client.MessageReceived += new LevellingModule(services.GetRequiredService<IUnitOfWork>()).OnMessageReceivedEvent;
        await client.LoginAsync(TokenType.Bot, configuration.GetSection("BotToken").Value);
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }
    
    private static readonly DiscordSocketConfig DiscordConfig = new() {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildEmojis
    };
    
    private static ServiceProvider ConfigureServices(IConfiguration configuration) {
        return new ServiceCollection()
            .AddDbContext<BotDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")))
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddSingleton<DiscordShardedClient>(x => new DiscordShardedClient(DiscordConfig))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>()))
            .AddSingleton<InteractionHandler>()
            .BuildServiceProvider();
    }
}
