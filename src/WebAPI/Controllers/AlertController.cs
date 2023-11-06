using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Alerts.Commands.DeleteAlert;
using SiteWatcher.Application.Alerts.Commands.GetAlertDetails;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Alerts.Commands.SearchAlerts;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI.Filters.Cache;

namespace SiteWatcher.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("alert")]
public class AlertController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlertController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAlert([FromServices] CreateAlertCommandHandler handlerHandler,
        CreateAlertCommand request, CancellationToken ct)
    {
        var res = await handlerHandler.Handle(request, ct);
        return res.Error != null ? res.Error.ToActionResult() : Created(string.Empty, res.Value);
    }

    [HttpGet]
    [CacheFilter]
    public async Task<IActionResult> GetUserAlerts([FromServices] GetUserAlertsQueryHandler handler,
        [FromQuery] GetUserAlertsQuery request, CancellationToken ct)
    {
        var res = await handler.Handle(request, ct);
        return res.Error != null ? res.Error.ToActionResult() : Ok(res.Value);
    }

    [HttpGet("{AlertId}/details")]
    [CacheFilter]
    public async Task<IActionResult> GetAlertDetails([FromRoute] GetAlertDetailsQuery request, CancellationToken ct) =>
        Ok(await _mediator.Send(request, ct));

    [HttpDelete("{AlertId}")]
    public async Task<IActionResult> DeleteAlert([FromServices] DeleteAlertCommandHandler handler,
        [FromRoute] DeleteAlertCommand request, CancellationToken ct)
    {
        var res = await handler.Handle(request, ct);
        return res.Error != null ? res.Error.ToActionResult() : NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAlert([FromServices] UpdateAlertCommandHandler commandHandler,
        [FromBody] UpdateAlertCommmand request, CancellationToken ct)
    {
        var res = await commandHandler.Handle(request, ct);
        return res.Error != null ? res.Error.ToActionResult() : Ok(res.Value);
    }

    [HttpGet("search")]
    [CacheFilter]
    public async Task<IActionResult> SearchAlerts([FromServices] SearchAlertQueryHandler handler,
        [FromQuery] SearchAlertQuery request, CancellationToken ct)
    {
        var res = await handler.Handle(request, ct);
        return res.Error != null ? res.Error.ToActionResult() : Ok(res.Value);
    }
}