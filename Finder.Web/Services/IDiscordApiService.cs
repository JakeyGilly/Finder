using Discord.Rest;

namespace Finder.Web.Services;

public interface IDiscordApiService {
    Task<T?> ExecuteAsUserAsync<T>(Func<DiscordRestClient, Task<T>> action);
    Task<T?> ExecuteAsBotAsync<T>(Func<DiscordRestClient, Task<T>> action);
}