using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Finder.Db.UnitOfWork;

namespace Finder.Web.Controllers;

[Authorize]
[Route("user/settings")]
public class UserSettingsController(ILogger<UserSettingsController> logger, IWebUnitOfWork unitOfWork) : Controller {
    private readonly ILogger<UserSettingsController> _logger = logger;

    [Route("")]
    public IActionResult Index() {
        return View("Index");
    }
    
    [HttpPost("")]
    public async Task<IActionResult> Update(string darkMode, string devMode) {
        var userId = ulong.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        unitOfWork.UserSettings.AddItem(new() {
            UserId = userId,
            Setting = "DarkMode",
            Value = darkMode
        });
        unitOfWork.UserSettings.AddItem(new() {
            UserId = userId,
            Setting = "DevMode",
            Value = devMode
        });
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}