using Domain.DTOs.Alert;
using Domain.DTOs.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Alerts.Commands.GetAlertDetails;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.WebAPI.DTOs.ViewModels;
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
    public async Task<IActionResult> CreateAlert(CreateAlertCommand command)
    {
        var appResult = await _mediator.Send(command);
        return Ok(new WebApiResponse<DetailedAlertView>().SetResult(appResult.Value!));
    }

    [HttpGet]
    [CacheFilter]
    public async Task<IActionResult> GetUserAlerts([FromQuery] GetUserAlertsCommand command,
        CancellationToken cancellationToken)
    {
        var appResult = await _mediator.Send(command, cancellationToken);
        return Ok(new WebApiResponse<PaginatedList<SimpleAlertView>>().SetResult(appResult.Value!));
    }

    [HttpGet("{AlertId}/details")]
    [CacheFilter]
    public async Task<IActionResult> GetAlertDetails([FromRoute] GetAlertDetailsCommand command,
        CancellationToken cancellationToken)
    {
        var appResult = await _mediator.Send(command, cancellationToken);
        return Ok(new WebApiResponse<AlertDetails>().SetResult(appResult.Value!));
    }
}