using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Finder.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DevController : Controller {
    private readonly ILogger<DevController> _logger;
    public DevController(ILogger<DevController> logger) {
        _logger = logger;
    }
    
    [Route("token")]
    public async Task<IActionResult> Token() {
        return Ok(new {
            access_token = await HttpContext.GetTokenAsync("access_token"),
            refresh_token = await HttpContext.GetTokenAsync("refresh_token"),
            token_type = await HttpContext.GetTokenAsync("token_type")
        });
    }
    
    [Route("claims")]
    public IActionResult Claims() {
        return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
    }
}