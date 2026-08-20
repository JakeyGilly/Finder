using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Finder.Db;
using Finder.Db.UnitOfWork;

namespace Finder.Web;

 class Program {
    static void Main(string[] args) {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
        var configurationBuilder = new ConfigurationBuilder()
            .AddEnvironmentVariables();
        if (environment == "Development") {
            configurationBuilder.AddUserSecrets<Program>();
        }
        var configuration = configurationBuilder.Build();
        
        var connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("Configuration error: 'ConnectionStrings:PostgreSQL' is required.");
        var discordClientId = configuration["DiscordClientId"] 
            ?? throw new InvalidOperationException("Configuration error: 'DiscordClientId' is required.");
        var discordClientSecret = configuration["DiscordClientSecret"] 
            ?? throw new InvalidOperationException("Configuration error: 'DiscordClientSecret' is required.");
        var discordBotToken = configuration["DiscordBotToken"] 
            ?? throw new InvalidOperationException("Configuration error: 'DiscordBotToken' is required.");

        var botOwnerIds = configuration["BotOwnerIds"]?.Split(',').Select(id => ulong.Parse(id)).ToList() ?? [];
        
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<FinderDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")), ServiceLifetime.Transient);
        builder.Services.AddTransient<IWebUnitOfWork, WebUnitOfWork>();
        builder.Services.AddAuthentication(options => options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            }).AddDiscord(options => {
                options.Scope.Add("identify");
                options.Scope.Add("guilds");
                options.Prompt = "none";
                options.ClientId = discordClientId;
                options.ClientSecret = discordClientSecret;
                options.SaveTokens = true;
                options.Events = new OAuthEvents {
                    OnCreatingTicket = context => {
                        if (!ulong.TryParse(context.Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Task.CompletedTask;
                        if (botOwnerIds.Count == 0 || !botOwnerIds.Contains(userId)) {
                            context.Identity.AddClaim(new Claim("IsBotOwner", "false"));
                        } else {
                            context.Identity.AddClaim(new Claim("IsBotOwner", "true"));
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        builder.Services.AddAuthorization(options => options.AddPolicy("IsBotOwner", policy => policy.RequireClaim("IsBotOwner", "true", "false")));
        builder.Services.AddHttpClient();
        builder.Services.AddControllersWithViews();
        var app = builder.Build();
        if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
        app.UseRouting();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        app.Run();
    }
}

