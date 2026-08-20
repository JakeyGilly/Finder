using Microsoft.AspNetCore.Mvc;
namespace Finder.Web.Controllers;

[Route("")]
public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    public HomeController(ILogger<HomeController> logger) {
        _logger = logger;
    }

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