using Domain.DTOs.Alert;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace SiteWatcher.WebAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("alert")]
public class AlertController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlertController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateAlert(CreateAlertCommand commandBase)
    {
        var appResult = await _mediator.Send(commandBase);
        return Ok(new WebApiResponse<DetailedAlertView>().SetResult(appResult.Value!));
    }
}
