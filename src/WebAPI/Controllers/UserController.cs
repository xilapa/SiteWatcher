using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Application.Users.Commands.ConfirmEmail;
using SiteWatcher.Application.Users.Commands.DeactivateAccount;
using SiteWatcher.Application.Users.Commands.DeleteUser;
using SiteWatcher.Application.Users.Commands.GetUserinfo;
using SiteWatcher.Application.Users.Commands.LogoutUserOfAllDevices;
using SiteWatcher.Application.Users.Commands.ReactivateAccount;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Application.Users.Commands.SendEmailConfirmation;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.WebAPI.Filters;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI.Filters.Cache;

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

    [HttpGet]
    [Authorize]
    [CacheFilter(ResponseCache: false)]
    public async Task<IActionResult> GetUserInfo([FromRoute] GetUserInfoQuery request, CancellationToken ct)
    {
        var res = await _mediator.Send(request, ct);
        if (res == null) return NotFound();
        return Ok(res);
    }

    [HttpPost]
    [CommandValidationFilter]
    [Route("register")]
    [Authorize(Policy = Policies.ValidRegisterData)]
    public async Task<IActionResult> Register(RegisterUserCommand request)
    {
        request.GoogleId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.GoogleId)?.Value;
        request.AuthEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Email)?.Value;
        RegisterUserResult commandResult = await _mediator.Send(request);
        return commandResult switch
        {
            AlreadyExists => Conflict(),
            Registered registered => Created(string.Empty, registered),
            _ => throw new ArgumentOutOfRangeException(nameof(commandResult))
        };
    }

    [Authorize]
    [HttpPut("resend-confirmation-email")]
    public async Task ResendConfirmationEmail() =>
        await _mediator.Send(new SendEmailConfirmationCommand());

    [AllowAnonymous]
    [HttpPut("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand request)
    {
        var commandResult = await _mediator.Send(request);
        return commandResult.ToActionResult();
    }

    [Authorize]
    [HttpPut]
    [CommandValidationFilter]
    public async Task<IActionResult> UpdateUser(UpdateUserCommand request)
    {
        var commandResult = await _mediator.Send(request);
        return commandResult.ToActionResult<UpdateUserResult>();
    }

    [Authorize]
    [HttpPut("deactivate")]
    public async Task DeactivateAccount() =>
        await _mediator.Send(new DeactivateAccountCommand());

    [AllowAnonymous]
    [CommandValidationFilter]
    [HttpPut("send-reactivate-account-email")]
    public async Task SendRectivateAccountEmail(SendReactivateAccountEmailCommand request) =>
        await _mediator.Send(request);

    [AllowAnonymous]
    [HttpPut("reactivate-account")]
    public async Task<IActionResult> ReactivateAccount(ReactivateAccountCommand request)
    {
        var commandResult = await _mediator.Send(request);
        return commandResult.ToActionResult();
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