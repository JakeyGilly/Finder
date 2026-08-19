namespace Finder.Bot.Db.Repositories.Addons;

public interface IAddonsRepository {
    Task<bool> AddonEnabled(ulong guildId, string addonName);
}