using System.Net;
using System.Text;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace Finder.Web.Services;

public class DiscordApiService(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration
) : IDiscordApiService {
    private static readonly DiscordRestClient BotClient = new();
    private readonly string _botToken = configuration["DiscordBotToken"]!;
    private readonly string _clientId = configuration["DiscordClientId"]!;
    private readonly string _clientSecret = configuration["DiscordClientSecret"]!;
    private static readonly string BaseUrl = "https://discord.com/api";
    
    public async Task<T?> ExecuteAsUserAsync<T>(Func<DiscordRestClient, Task<T>> action) {
        return await AccessTokenRefreshWrapper(async () => {
            await using var client = new DiscordRestClient();
            var accessToken = await httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken)) return default;
            await client.LoginAsync(TokenType.Bearer, accessToken);
            return await action(client);
        });
    }

    public async Task<T?> ExecuteAsBotAsync<T>(Func<DiscordRestClient, Task<T>> action) {
        if (BotClient.LoginState != LoginState.LoggedIn) {
            await BotClient.LoginAsync(TokenType.Bot, _botToken);
        }
        return await action(BotClient);
    }
    
    private async Task<T?> AccessTokenRefreshWrapper<T>(Func<Task<T?>> initialRequest) {
        try
        {
            return await initialRequest();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized) {
            var refreshToken = await httpContextAccessor.HttpContext!.GetTokenAsync("refresh_token");
            if (string.IsNullOrEmpty(refreshToken)) return default;
            await RefreshAccessToken(refreshToken);
            return await initialRequest();
        }
    }
    
    private async Task RefreshAccessToken(string refreshToken) {
        var requestData = new Dictionary<string, string> {
            ["grant_type"] = "refresh_token", 
            ["refresh_token"] = refreshToken,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/oauth2/token") {
            Content = new FormUrlEncodedContent(requestData)
        };
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return;
        var responseString = await response.Content.ReadAsStringAsync();
        var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
        if (responseData != null) {
            var authInfo = await httpContextAccessor.HttpContext!.AuthenticateAsync();
            if (authInfo.Properties != null) {
                if (responseData.TryGetValue("access_token", out var newAccessToken)) authInfo.Properties.UpdateTokenValue("access_token", newAccessToken);
                if (responseData.TryGetValue("refresh_token", out var newRefreshToken)) authInfo.Properties.UpdateTokenValue("refresh_token", newRefreshToken);
                if (authInfo.Principal != null) await httpContextAccessor.HttpContext!.SignInAsync(authInfo.Principal, authInfo.Properties);
            }
        }
    }
}