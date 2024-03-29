﻿using Mediator;
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
    [CommandValidationFilter]
    public async Task<IActionResult> CreateAlert(CreateAlertCommand request, CancellationToken ct) =>
        Created(string.Empty, await _mediator.Send(request, ct));

    [HttpGet]
    [CacheFilter]
    public async Task<IActionResult> GetUserAlerts([FromQuery] GetUserAlertsQuery request, CancellationToken ct)
    {
        var commandResult = await _mediator.Send(request, ct);
        return commandResult.ToActionResult<PaginatedList<SimpleAlertView>>();
    }

    [HttpGet("{AlertId}/details")]
    [CacheFilter]
    public async Task<IActionResult> GetAlertDetails([FromRoute] GetAlertDetailsQuery request, CancellationToken ct) =>
        Ok(await _mediator.Send(request, ct));

    [HttpDelete("{AlertId}")]
    public async Task<IActionResult> DeleteAlert([FromRoute] DeleteAlertCommand request,
        CancellationToken cancellationToken)
    {
        var commandResult = await _mediator.Send(request, cancellationToken);
        return commandResult.ToActionResult();
    }

    [HttpPut]
    [CommandValidationFilter]
    public async Task<IActionResult> UpdateAlert([FromBody] UpdateAlertCommmand request, CancellationToken ct)
    {
        var commandResult = await _mediator.Send(request, ct);
        return commandResult.ToActionResult<DetailedAlertView>();
    }

    [HttpGet("search")]
    [CacheFilter]
    [CommandValidationFilter]
    public async Task<IActionResult> SearchAlerts([FromQuery] SearchAlertQuery request, CancellationToken ct) =>
        Ok(await _mediator.Send(request, ct));
}