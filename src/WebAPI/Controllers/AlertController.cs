using Domain.DTOs.Alert;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using SiteWatcher.WebAPI.Filters;

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
    public async Task<IActionResult> CreateAlert(CreateAlertCommand commandBase)
    {
        var appResult = await _mediator.Send(commandBase);
        return Ok(new WebApiResponse<DetailedAlertView>().SetResult(appResult.Value!));
    }

    [HttpGet]
    public async Task<IActionResult> GetUserAlerts([FromQuery] GetUserAlertsCommand command, CancellationToken cancellationToken)
    {
        var appResult = await _mediator.Send(command, cancellationToken);
        return Ok(new WebApiResponse<IEnumerable<SimpleAlertView>>().SetResult(appResult.Value!));
    }
}
