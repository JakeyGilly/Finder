using Finder.Bot.Models.Data;

namespace Finder.Bot.Db.Repositories.Levelling;

public interface ILevellingRepository: IRepository<LevellingModel> {
    Task<LevellingModel> GetLevellingForGuildAsync(ulong guildId, ulong userId);
}