using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Infra;
using SiteWatcher.WebAPI.Filters;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    private readonly DatabaseMigrator _migrator;

    public HomeController(DatabaseMigrator migrator)
    {
        _migrator = migrator;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get() => Ok();

    [ApiKey]
    [HttpPost("migrate")]
    public async Task<IActionResult> ApplyPendingMigrations()
    {
        var result = await _migrator.Migrate();
        return Ok(result);
    }
}