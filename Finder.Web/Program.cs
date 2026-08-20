using Finder.Web.Database;
using Finder.Web.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
namespace Finder.Web;

public static class Program {
    public static void Main(string[] args) {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID"))) {
            Console.WriteLine("Environment variable DISCORD_CLIENT_ID is not set.");
            return;
        }
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET"))) {
            Console.WriteLine("Environment variable DISCORD_CLIENT_SECRET is not set.");
            return;
        }
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"))) {
            Console.WriteLine("Environment variable DISCORD_BOT_TOKEN is not set.");
            return;
        }
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"))) {
            Console.WriteLine("Environment variable DB_CONNECTION_STRING is not set.");
            return;
        }
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!), ServiceLifetime.Transient);
        builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        builder.Services.AddAuthentication(options => options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            }).AddDiscord(options => {
                options.Scope.Add("identify");
                options.Scope.Add("guilds");
                options.Prompt = "none";
                options.ClientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID")!;
                options.ClientSecret = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET")!;
                options.SaveTokens = true;
                options.Events = new OAuthEvents {
                    OnCreatingTicket = context => {
                        List<string> ownerIds = Environment.GetEnvironmentVariable("BOT_OWNER_IDS")?.Split(',').ToList() ?? new List<string>();
                        if (!ulong.TryParse(context.Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Task.CompletedTask;
                        if (ownerIds.IsNullOrEmpty() || !ownerIds.Contains(userId.ToString())) {
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

