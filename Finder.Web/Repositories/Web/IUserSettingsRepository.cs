using Finder.Web.Models.Data.Web;
namespace Finder.Web.Repositories.Web;

public interface IUserSettingsRepository : IRepository<UserSettingsModel> {
    Task<string?> GetUserSettingAsync(ulong userId, string key);
    Task AddSettingAsync(ulong userId, string key, string value);
    Task<bool> UserSettingExists(ulong userId, string key);
}