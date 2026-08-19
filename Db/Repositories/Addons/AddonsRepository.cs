using Finder.Bot.Db.Models;

namespace Finder.Bot.Db.Repositories.Addons;

public class AddonsRepository(BotDbContext context) : EfRepository<AddonsModel>(context), IAddonsRepository {
    public async Task<bool> AddonEnabledInGuildAsync(ulong guildId, Enums.Addons addonName) {
        var addonData = await GetItemAsync((m) => m.GuildId == guildId && m.Addon == addonName);
        return addonData is { Enabled: true };
    }
    
    public async Task<List<Enums.Addons>> GetAddonsForGuildAsync(ulong guildId) {
        return [.. (await GetItemsAsync((m) => m.GuildId == guildId && m.Enabled)).Select(m => m.Addon)];
    }
}