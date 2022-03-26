using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromServices] AppSettings settings, [FromServices] ILogger<HomeController> logger)
    {
        var originIp = Request.HttpContext.Connection.RemoteIpAddress;  
        logger.LogInformation("Someone from {ip} tried to access the main api url at {date}", originIp, DateTime.UtcNow);
        return Redirect(settings.FrontEndUrl);
    }
}