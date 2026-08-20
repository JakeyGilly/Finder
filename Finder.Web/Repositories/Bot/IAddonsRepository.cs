using Finder.Web.Models.Data.Bot;
namespace Finder.Web.Repositories.Bot;

public interface IAddonsRepository : IRepository<AddonsModel> {
    Task<string?> GetAddonAsync(ulong guildId, string key);
    Task AddAddonAsync(ulong guildId, string key, string value);
    Task RemoveAddonAsync(ulong guildId, string key);
    Task<bool> AddonEnabled(ulong guildId, string key);
}