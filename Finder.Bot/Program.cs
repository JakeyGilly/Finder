using System.Runtime.InteropServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Finder.Bot.Handlers;
using Finder.Bot.Modules;
using Finder.Bot.Modules.Addons;
using Finder.Bot.Modules.Helpers;
using Finder.Bot.Services;
using Microsoft.Extensions.Logging;
using Finder.Db;
using Finder.Db.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Finder.Bot;

class Program {
    static async Task Main() {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
        var configurationBuilder = new ConfigurationBuilder()
            .AddEnvironmentVariables();
        if (environment == "Development") {
            configurationBuilder.AddUserSecrets<Program>();
        }
        var configuration = configurationBuilder.Build();
        
        var connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("Configuration error: 'ConnectionStrings:PostgreSQL' is required.");
        var botToken = configuration["BotToken"] 
            ?? throw new InvalidOperationException("Configuration error: 'BotToken' is required.");
        
        await using ServiceProvider services = ConfigureServices(connectionString);
        using (var scope = services.CreateScope()) {
            var dbContext = scope.ServiceProvider.GetRequiredService<FinderDbContext>();
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Database migrations applied successfully.");
        }
        DiscordShardedClient client = services.GetRequiredService<DiscordShardedClient>();
        InteractionService commands = services.GetRequiredService<InteractionService>();
        InteractionHandler handler = services.GetRequiredService<InteractionHandler>();
        await handler.InitializeAsync();
        client.Log += LoggingService.LogAsync;
        commands.Log += LoggingService.LogAsync;
        var unbanTimer = new UnBanMuteTimer(client, services);
        unbanTimer.StartTimer();
        var countdownTimer = new CountdownTimer(client, services);
        countdownTimer.StartTimer();
        client.ReactionAdded += TicTacToeModule.OnReactionAddedEvent;
        client.ReactionAdded += new ModerationModule(services.GetRequiredService<IBotUnitOfWork>()).OnReactionAddedEvent;
        client.ButtonExecuted += new PollModule(services.GetRequiredService<IBotUnitOfWork>()).OnButtonExecutedEvent;
        client.ButtonExecuted += new TicketingModule(services.GetRequiredService<IBotUnitOfWork>()).OnButtonExecutedEvent;
        client.MessageReceived += new LevellingModule(services.GetRequiredService<IBotUnitOfWork>()).OnMessageReceivedEvent;
        client.InteractionCreated += new CodeModule(services.GetRequiredService<Judge0Service>()).OnModalInteractionAsync;
        await client.LoginAsync(TokenType.Bot, botToken);
        await client.StartAsync();
        
        var exitSignal = new TaskCompletionSource();
        using var sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx => {
            ctx.Cancel = true;
            exitSignal.TrySetResult();
        });
        using var sigint = PosixSignalRegistration.Create(PosixSignal.SIGINT, ctx => {
            ctx.Cancel = true;
            exitSignal.TrySetResult();
        });
        await exitSignal.Task;
        Console.WriteLine("Stop signal received. Initiating shutdown...");

        unbanTimer.Dispose();
        countdownTimer.Dispose();
        await services.GetRequiredService<Judge0Service>().DisposeAsync(); 
        await client.StopAsync();
        Console.WriteLine("Bot shut down cleanly.");
    }
    
    private static ServiceProvider ConfigureServices(string connectionString) {
        
        return new ServiceCollection()
            .AddLogging(builder => {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            })
            .AddDbContext<FinderDbContext>(options => options.UseNpgsql(connectionString))
            .AddScoped<IBotUnitOfWork, BotUnitOfWork>()
            .AddSingleton<DiscordShardedClient>(x => new DiscordShardedClient(new DiscordSocketConfig() {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildEmojis
            }))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>()))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<Judge0Service>()
            .BuildServiceProvider();
    }
}
