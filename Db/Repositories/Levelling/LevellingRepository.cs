using Finder.Bot.Models.Data;
using Microsoft.Azure.Cosmos;

namespace Finder.Bot.Db.Repositories.Levelling;

public class LevellingRepository(CosmosClient dbClient, string databaseName) : CosmosRepository<LevellingModel>(dbClient, databaseName, "levelling"), ILevellingRepository {
    public async Task<LevellingModel> GetLevellingForGuildAsync(ulong guildId, ulong userId) {
        var levellingData = await GetItemAsync(LevellingModel.FormatId(guildId, userId));
        return levellingData ?? new LevellingModel() {
            Id = LevellingModel.FormatId(guildId, userId),
            GuildId = guildId,
            UserId = userId,
            Level = 0,
            Exp = 0
        };
    }
}