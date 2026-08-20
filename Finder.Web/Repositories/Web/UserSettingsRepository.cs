using Finder.Web.Database;
using Finder.Web.Models.Data.Web;
namespace Finder.Web.Repositories.Web;

public class UserSettingsRepository : Repository<UserSettingsModel>, IUserSettingsRepository {
    public UserSettingsRepository(ApplicationContext context) : base(context) { }

    public async Task<string?> GetUserSettingAsync(ulong userId, string key) {
        var settings = await GetAsync(userId);
        return settings?.Settings[key];
    }

    public async Task AddSettingAsync(ulong userId, string key, string value) {
        var userSettings = await GetAsync(userId);
        if (userSettings == null) {
            await AddAsync(new UserSettingsModel {
                UserId = (long)userId,
                Settings = new Dictionary<string, string> {
                    { key, value }
                }
            });
            return;
        }
        if (userSettings.Settings.ContainsKey(key)) {
            userSettings.Settings[key] = value;
        } else {
            userSettings.Settings.Add(key, value);
        }
        Context.Set<UserSettingsModel>().Update(userSettings);
    }

    public async Task<bool> UserSettingExists(ulong userId, string key) {
        var settings = await GetAsync(userId);
        return settings?.Settings.ContainsKey(key) ?? false;
    }
}