using Finder.Bot.Models.Data;

namespace Finder.Bot.Db.Repositories.Addons;

public interface IAddonsRepository: IRepository<AddonsModel> {
    Task<bool> AddonEnabledInGuildAsync(ulong guildId, Enums.Addons addon);
    Task<Dictionary<Enums.Addons, bool>> GetAddonsForGuildAsync(ulong guildId);
    Task UpdateAddonForGuildAsync(ulong guildId, Enums.Addons addon, bool enabled);
}