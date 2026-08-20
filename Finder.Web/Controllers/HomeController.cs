using Microsoft.AspNetCore.Mvc;
namespace Finder.Web.Controllers;

[Route("")]
public class HomeController(ILogger<HomeController> logger) : Controller {
    [Route("")]
    public IActionResult Index() {
        return View("Index");
    }

    [Route("privacy")]
    public IActionResult Privacy() {
        ViewBag.DarkMode = false;
        return View("Privacy");
    }
}