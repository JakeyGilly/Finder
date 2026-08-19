using Finder.Bot.Db.Models;

namespace Finder.Bot.Db.Repositories.Addons;

public interface IAddonsRepository: IRepository<AddonsModel> {
    Task<bool> AddonEnabledInGuildAsync(ulong guildId, Enums.Addons addon);
    Task<List<Enums.Addons>> GetAddonsForGuildAsync(ulong guildId);
}