using Finder.Bot.Db;

namespace Finder.Bot.Models.Data;

public class LevellingModel: ICosmosItem {
    // composite key of guildId and userId
    public string Id {
        get => FormatId(GuildId, UserId);
        set { }
    }
    
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    
    public static string FormatId(ulong guildId, ulong userId) => $"{guildId}_{userId}";
}