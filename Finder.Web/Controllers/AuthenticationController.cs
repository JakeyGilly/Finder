using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
namespace Finder.Web.Controllers; 

public class AuthenticationController : Controller {
    ILogger<AuthenticationController> _logger;
    public AuthenticationController(ILogger<AuthenticationController> logger) {
        _logger = logger;
    }
    
    [Route("login")]
    public IActionResult LogIn() {
        return Challenge(new AuthenticationProperties { RedirectUri = "/" }, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [Route("logout")]
    public async Task<IActionResult> LogOut() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}