using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Application.Users.Commands.ConfirmEmail;
using SiteWatcher.Application.Users.Commands.DeactivateAccount;
using SiteWatcher.Application.Users.Commands.DeleteUser;
using SiteWatcher.Application.Users.Commands.LogoutUserOfAllDevices;
using SiteWatcher.Application.Users.Commands.ReactivateAccount;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Application.Users.Commands.SendEmailConfirmation;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using SiteWatcher.WebAPI.Filters;

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
    [CommandValidationFilter]
    [Route("register")]
    [Authorize(Policy = Policies.ValidRegisterData)]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var response = new WebApiResponse<RegisterUserResult>();
        var appResult = await _mediator.Send(command);

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
    [CommandValidationFilter]
    public async Task<IActionResult> UpdateUser(UpdateUserCommand command)
    {
        var response = new WebApiResponse<UpdateUserResult>();
        var appResult = await _mediator.Send(command);

        if (!appResult.Success)
            return BadRequest(response.AddMessages(appResult.Errors));

        return Ok(response.SetResult(appResult.Value!));
    }

    [Authorize]
    [HttpPut("deactivate")]
    public async Task DeactivateAccount() =>
        await _mediator.Send(new DeactivateAccountCommand());

    [AllowAnonymous]
    [CommandValidationFilter]
    [HttpPut("send-reactivate-account-email")]
    public async Task SendRectivateAccountEmail(SendReactivateAccountEmailCommand command) =>
        await _mediator.Send(command);

    [AllowAnonymous]
    [HttpPut("reactivate-account")]
    public async Task<IActionResult> ReactivateAccount(ReactivateAccountCommand reactivateAccountCommand)
    {
        var appResult = await _mediator.Send(reactivateAccountCommand);

        if (appResult.Success)
            return Ok();

        var response = new WebApiResponse<object>();
        return BadRequest(response.AddMessages(appResult.Errors));
    }

    [Authorize]
    [HttpDelete]
    public async Task DeleteAccount() =>
        await _mediator.Send(new DeleteAccountCommand());

    [Authorize]
    [HttpPost("logout-all-devices")]
    public async Task LogoutOfAllDevices() =>
        await _mediator.Send(new LogoutUserOfAllDevicesCommand());
}