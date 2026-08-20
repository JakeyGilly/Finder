using Finder.Db.Models.Web;
using Finder.Db.Repositories;

namespace Finder.Db.UnitOfWork;

public interface IWebUnitOfWork: IBotUnitOfWork {
    IRepository<UserSettingsModel> UserSettings { get; }
}