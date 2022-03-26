using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Infra.Data;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using SiteWatcher.WebAPI.Filters;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Controllers;


[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> logger;
    private readonly AppSettings settings;
    private readonly DatabaseMigrator migrator;

    public HomeController(ILogger<HomeController> logger, AppSettings settings, DatabaseMigrator migrator)
    {
        this.logger = logger;
        this.settings = settings;
        this.migrator = migrator;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var originIp = Request.HttpContext.Connection.RemoteIpAddress;  
        logger.LogInformation("Someone from {ip} tried to access the main api url at {date}", originIp, DateTime.UtcNow);
        return Redirect(settings.FrontEndUrl);
    }

    [ApiKey]
    [HttpPost]
    [Route("migrate")]
    public async Task<IActionResult> ApplyPendingMigrations()
    {
        var result = await migrator.Migrate();
        return Ok(new WebApiResponse<string>(result));
    }
}