using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Finder.Db.UnitOfWork;

namespace Finder.Web.Controllers;

[Authorize]
[Route("user/settings")]
public class UserSettingsController(ILogger<UserSettingsController> logger, IWebUnitOfWork unitOfWork) : Controller {
    [Route("")]
    public IActionResult Index() {
        return View("Index");
    }
    
    [HttpPost("")]
    public async Task<IActionResult> Update(string darkMode, string devMode) {
        ulong.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
        Console.WriteLine($"User {userId} updated settings: DarkMode={darkMode}, DevMode={devMode}");
        darkMode = string.IsNullOrEmpty(darkMode) ? "false" : darkMode;
        devMode = string.IsNullOrEmpty(devMode) ? "false" : devMode;
        var currentDarkMode =
            await unitOfWork.UserSettings.GetItemAsync(m => m.UserId == userId && m.Setting == "DarkMode");
        var currentDevMode =
            await unitOfWork.UserSettings.GetItemAsync(m => m.UserId == userId && m.Setting == "DevMode");
        if (currentDarkMode == null) {
            unitOfWork.UserSettings.AddItem(currentDarkMode = new() {
                UserId = userId,
                Setting = "DarkMode"
            });
        }
        currentDarkMode.Value = darkMode;
        if (currentDevMode == null) {
            unitOfWork.UserSettings.AddItem(currentDevMode = new() {
                UserId = userId,
                Setting = "DevMode"
            });
        }
        currentDevMode.Value = devMode;
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}