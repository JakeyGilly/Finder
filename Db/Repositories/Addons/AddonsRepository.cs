using Finder.Bot.Models.Data;
using Microsoft.Azure.Cosmos;

namespace Finder.Bot.Db.Repositories.Addons;

public class AddonsRepository(CosmosClient dbClient, string databaseName) : CosmosRepository<AddonsModel>(dbClient, databaseName, "addons"), IAddonsRepository {
    public async Task<bool> AddonEnabledInGuildAsync(ulong guildId, Enums.Addons addonName) {
        var addonData = await GetItemAsync(guildId.ToString());
        return addonData != null && addonData.Addons.TryGetValue(addonName, out bool enabled) && enabled;
    }
    
    public async Task<Dictionary<Enums.Addons, bool>> GetAddonsForGuildAsync(ulong guildId) {
        var addonData = await GetItemAsync(guildId.ToString());
        return addonData?.Addons ?? new Dictionary<Enums.Addons, bool>();
    }
    
    public async Task UpdateAddonForGuildAsync(ulong guildId, Enums.Addons addon, bool enabled) {
        var addonData = await GetItemAsync(guildId.ToString()) ?? new AddonsModel {
            Id = guildId.ToString(),
            Addons = new Dictionary<Enums.Addons, bool>()
        };
        
        addonData.Addons[addon] = enabled;
        await UpsertItemAsync(guildId.ToString(), addonData);
    }
}