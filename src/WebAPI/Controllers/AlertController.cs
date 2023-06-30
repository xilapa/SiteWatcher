using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Alerts.Commands.DeleteAlert;
using SiteWatcher.Application.Alerts.Commands.GetAlertDetails;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Alerts.Commands.SearchAlerts;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI.Filters;
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
    public async Task<IActionResult> CreateAlert(CreateAlertCommand cmmd)
    {
        var res = await _mediator.Send(cmmd);
        return res.ToActionResult<DetailedAlertView>(r => Created(string.Empty, r));
    }

    [HttpGet]
    [CacheFilter]
    public async Task<IActionResult> GetUserAlerts([FromQuery] GetUserAlertsCommand command,
        CancellationToken cancellationToken)
    {
        var commandResult = await _mediator.Send(command, cancellationToken);
        return commandResult.ToActionResult<PaginatedList<SimpleAlertView>>();
    }

    [HttpGet("{AlertId}/details")]
    [CacheFilter]
    public async Task<IActionResult> GetAlertDetails([FromRoute] GetAlertDetailsCommand command, CancellationToken ct) =>
        Ok(await _mediator.Send(command, ct));

    [HttpDelete("{AlertId}")]
    public async Task<IActionResult> DeleteAlert([FromRoute] DeleteAlertCommand command,
        CancellationToken cancellationToken)
    {
        var commandResult = await _mediator.Send(command, cancellationToken);
        return commandResult.ToActionResult();
    }

    [HttpPut]
    [CommandValidationFilter]
    public async Task<IActionResult> UpdateAlert([FromBody] UpdateAlertCommmand command, CancellationToken cancellationToken)
    {
        var commandResult = await _mediator.Send(command, cancellationToken);
        return commandResult.ToActionResult<DetailedAlertView>();
    }

    [HttpGet("search")]
    [CacheFilter]
    [CommandValidationFilter]
    public async Task<IActionResult> SearchAlerts([FromQuery] SearchAlertCommand command, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(command, cancellationToken));
}