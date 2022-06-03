using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Application.Users.Commands.ConfirmEmail;
using SiteWatcher.Application.Users.Commands.DeactivateAccount;
using SiteWatcher.Application.Users.Commands.DeleteUser;
using SiteWatcher.Application.Users.Commands.LogoutUserOfAllDevices;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Application.Users.Commands.SendEmailConfirmation;
using SiteWatcher.Application.Users.Commands.UpdateUser;
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

    [HttpPost]
    [Route("register")]
    [Authorize(Policy = Policies.ValidRegisterData)]
    public async Task<IActionResult> Register(RegisterUserCommand registerUserCommand)
    {
        var response = new WebApiResponse<RegisterUserResult>();
        var appResult = await _mediator.Send(registerUserCommand);

        if (!appResult.Success)
            return Conflict(response.AddMessages(appResult.Errors));

        return Created(string.Empty, response.SetResult(appResult.Value!));
    }

    [Authorize]
    [HttpPut("resend-confirmation-email")]
    public async Task ResendConfirmationEmail() =>
        await _mediator.Send(new SendEmailConfirmationCommand());

    [AllowAnonymous]
    [HttpPut("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand confirmEmailCommand)
    {
        var appResult = await _mediator.Send(confirmEmailCommand);

        if (appResult.Success)
            return Ok();

        var response = new WebApiResponse<object>();
        return BadRequest(response.AddMessages(appResult.Errors));
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateUser(UpdateUserCommand updateUserCommand)
    {
        var response = new WebApiResponse<UpdateUserResult>();
        var appResult = await _mediator.Send(updateUserCommand);

        if (!appResult.Success)
            return BadRequest(response.AddMessages(appResult.Errors));

        return Ok(response.SetResult(appResult.Value!));
    }

    [Authorize]
    [HttpPut("deactivate")]
    public async Task DeactivateAccount() =>
        await _mediator.Send(new DeactivateAccountCommand());

    [AllowAnonymous]
    [HttpPut("send-reactivate-account-email")]
    public async Task SendRectivateAccountEmail(SendReactivateAccountEmailCommand emailCommand) =>
        await _mediator.Send(emailCommand);

    [Authorize]
    [HttpDelete]
    public async Task DeleteAccount() =>
        await _mediator.Send(new DeleteAccountCommand());

    [Authorize]
    [HttpPost("logout-all-devices")]
    public async Task LogoutOfAllDevices() =>
        await _mediator.Send(new LogoutUserOfAllDevicesCommand());
}