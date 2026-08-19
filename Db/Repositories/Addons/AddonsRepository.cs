namespace Finder.Bot.Db.Repositories.Addons;

public class AddonsRepository(ICosmosDbService cosmosDbService) : IAddonsRepository {
    public async Task<bool> AddonEnabled(ulong guildId, string addonName) {
        var addonData = await cosmosDbService.GetItemAsync(guildId.ToString());
        return addonData != null && addonData.Addons.TryGetValue(addonName, out bool enabled) && enabled;
    }
}