using Finder.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Finder.Web.Controllers;

[Authorize]
[Route("user/settings")]
public class UserSettingsController : Controller {
    private readonly ILogger<UserSettingsController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public UserSettingsController(ILogger<UserSettingsController> logger, IUnitOfWork unitOfWork) {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    [Route("")]
    public IActionResult Index() {
        return View("Index");
    }
    
    [HttpPost("")]
    public async Task<IActionResult> Update(string darkMode, string devMode) {
        var userId = ulong.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        await _unitOfWork.UserSettings.AddSettingAsync(userId, "DarkMode", darkMode);
        await _unitOfWork.UserSettings.AddSettingAsync(userId, "DevMode", devMode);
        await _unitOfWork.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}