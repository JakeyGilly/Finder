using Finder.Web.Models.DiscordAPIModels;
using Finder.Web.Models.DTO;
using Finder.Web.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using System.Text;
namespace Finder.Web.Controllers;

[Authorize]
[Route("dashboard")]
public class DashboardController : Controller {
    private readonly ILogger<DashboardController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUnitOfWork _unitOfWork;
    public DashboardController(ILogger<DashboardController> logger, IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork) {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _unitOfWork = unitOfWork;
    }
    
    [Route("")]
    public async Task<IActionResult> Index() {
        return View("Index", new DashboardSelectorDTO {
            BotGuilds =  JsonConvert.DeserializeObject<List<Guild>>(await (await AccessTokenRefreshWrapper(async () => await BotDiscordApiGet("users/@me/guilds"))).Content.ReadAsStringAsync()),
            UserGuilds =  JsonConvert.DeserializeObject<List<Guild>>(await (await AccessTokenRefreshWrapper(async () => await UserDiscordApiGet("users/@me/guilds"))).Content.ReadAsStringAsync()),
            UserProfile =  JsonConvert.DeserializeObject<User>(await (await AccessTokenRefreshWrapper(async () => await UserDiscordApiGet("users/@me"))).Content.ReadAsStringAsync())
        });
    }


    [Route("{id}")]
    public async Task<IActionResult> Guild(string id) {
        return View("Dashboard", new GuildDashboardDTO {
            Guild = JsonConvert.DeserializeObject<Guild>(
                await (await BotDiscordApiGet($"guilds/{id}", new Dictionary<string, string> {{"with_counts", "true"}})).Content.ReadAsStringAsync()),
            GuildMembers = JsonConvert.DeserializeObject<List<GuildMember>>(
                await (await BotDiscordApiGet($"guilds/{id}/members", new Dictionary<string, string> {{ "limit", "1000" }})).Content.ReadAsStringAsync()),
            GuildChannels = JsonConvert.DeserializeObject<List<GuildChannel>>(
                await (await BotDiscordApiGet($"guilds/{id}/channels", new Dictionary<string, string> {{ "limit", "1000" }})).Content.ReadAsStringAsync())
        });
    }
    
    [HttpPost("{id}/addons")]
    public async Task<IActionResult> Addons(string id, [FromForm] string ticTacToeAddon, [FromForm] string economyAddon, [FromForm] string levelingAddon, [FromForm] string ticketingAddon) {
        var guildId = ulong.Parse(id);
        await _unitOfWork.Addons.AddAddonAsync(guildId, "TicTacToe", ticTacToeAddon);
        await _unitOfWork.Addons.AddAddonAsync(guildId, "Economy", economyAddon);
        await _unitOfWork.Addons.AddAddonAsync(guildId, "Leveling", levelingAddon);
        await _unitOfWork.Addons.AddAddonAsync(guildId, "Ticketing", ticketingAddon);
        await _unitOfWork.SaveChangesAsync();
        return RedirectToAction("Guild", new { id });
    }
    
    [HttpPost("{id}/message")]
    public async Task<IActionResult> Message(string id, [FromForm] ulong channelId, [FromForm] string message) {
        await BotDiscordApiPost($"channels/{channelId}/messages", $"{{\"content\": \"{message}\"}}");
        return RedirectToAction("Guild", new { id });
    }
    
    [NonAction]
    private async Task<HttpResponseMessage> AccessTokenRefreshWrapper(Func<Task<HttpResponseMessage>> initialRequest) {
        var response = await initialRequest();
        if (response.StatusCode != HttpStatusCode.Unauthorized) return response;
        var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
        if (refreshToken == null) return response;
        await RefreshAccessToken(refreshToken);
        response = await initialRequest();
        return response;
    }
    [NonAction]
    private async Task<HttpResponseMessage> UserDiscordApiGet(string urlEndpoint, Dictionary<string, string>? queryParams = null) {
        var client = _httpClientFactory.CreateClient();
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        if (queryParams == null) return await client.GetAsync($"https://discord.com/api/{urlEndpoint}");
        return await client.GetAsync($"https://discord.com/api/{urlEndpoint}?{string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"))}");
    }
    [NonAction]
    private async Task<HttpResponseMessage> BotDiscordApiGet(string urlEndpoint, Dictionary<string, string>? queryParams = null) {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bot {Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN")}");
        if (queryParams == null) return await client.GetAsync($"https://discord.com/api/{urlEndpoint}");
        return await client.GetAsync($"https://discord.com/api/{urlEndpoint}?{string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"))}");
    }
    
    [NonAction]
    private async Task<HttpResponseMessage> BotDiscordApiPost(string urlEndpoint, string json, Dictionary<string, string>? queryParams = null) {
        var client = _httpClientFactory.CreateClient();
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Add("Authorization", $"Bot {Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN")}");
        if (queryParams == null) return await client.PostAsync($"https://discord.com/api/{urlEndpoint}", content);
        return await client.PostAsync($"https://discord.com/api/{urlEndpoint}?{string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"))}", content);
    }
    [NonAction]
    private async Task RefreshAccessToken(string refreshToken) {
        var client = _httpClientFactory.CreateClient();
        var requestData = new Dictionary<string, string> {
            ["grant_type"] = "refresh_token", 
            ["refresh_token"] = refreshToken,
            ["client_id"] = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID")!,
            ["client_secret"] = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET")!
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token") {
            Content = new FormUrlEncodedContent(requestData)
        };
        var response = await client.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
        if (responseData != null) {
            var newAccessToken = responseData.GetValueOrDefault("access_token");
            var newRefreshToken = responseData.GetValueOrDefault("refresh_token");
            var authInfo = await HttpContext.AuthenticateAsync();
            if (authInfo.Properties != null) {
                if (newAccessToken != null) authInfo.Properties.UpdateTokenValue("access_token", newAccessToken);
                if (newRefreshToken != null) authInfo.Properties.UpdateTokenValue("refresh_token", newRefreshToken);
                if (authInfo.Principal != null) await HttpContext.SignInAsync(authInfo.Principal, authInfo.Properties);
            }
        }
    }
}