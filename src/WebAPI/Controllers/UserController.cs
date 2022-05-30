using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SiteWatcher.Application.Users.Commands.LogoutUserOfAllDevices;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Domain.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("confirm-email")]
    public void ConfirmEmail()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("register")]
    [Authorize(Policy = Policies.ValidRegisterData)]
    public async Task<IActionResult> Register(RegisterUserCommand registerUserCommand)
    {
        var response = new WebApiResponse<string>();
        var result = await _mediator.Send(registerUserCommand);

        if (!result.Success)
            return Conflict(response.AddMessages(result.Errors));

        return Created(string.Empty, response.SetResult(result.Value));
    }

    [Authorize]
    [HttpPost("logout-all-devices")]
    public async Task LogoutOfAllDevices() =>
        await _mediator.Send(new LogoutUserOfAllDevicesCommand());
}