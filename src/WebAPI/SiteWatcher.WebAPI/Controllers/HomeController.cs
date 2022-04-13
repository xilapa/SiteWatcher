using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Data;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using SiteWatcher.WebAPI.Filters;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppSettings _settings;
    private readonly DatabaseMigrator _migrator;

    public HomeController(ILogger<HomeController> logger, AppSettings settings, DatabaseMigrator migrator)
    {
        _logger = logger;
        _settings = settings;
        _migrator = migrator;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // TODO: Arrumar ou remover, pois como está não funciona
        var originIp = Request.HttpContext.Connection.RemoteIpAddress;
        _logger.LogInformation("Someone from {Ip} tried to access the main api url at {Date}", originIp, DateTime.UtcNow);
        return Redirect(_settings.FrontEndUrl);
    }

    [ApiKey]
    [HttpPost]
    [Route("migrate")]
    public async Task<IActionResult> ApplyPendingMigrations()
    {
        var result = await _migrator.Migrate();
        return Ok(new WebApiResponse<string>(result));
    }
}