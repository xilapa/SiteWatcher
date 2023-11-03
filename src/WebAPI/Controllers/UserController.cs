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
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI.Filters.Cache;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    [HttpGet]
    [Authorize]
    [CacheFilter(ResponseCache: false)]
    public async Task<IActionResult> GetUserInfo([FromServices]GetUserInfoQueryHandler handler, [FromRoute] GetUserInfoQuery request, CancellationToken ct)
    {
        var res = await handler.Handle(request, ct);
        if (res == null) return NotFound();
        return Ok(res);
    }

    [HttpPost]
    [Route("register")]
    [Authorize(Policy = Policies.ValidRegisterData)]
    public async Task<IActionResult> Register([FromServices] RegisterUserCommandHandler handler,
        RegisterUserCommand request, CancellationToken ct)
    {
        request.GoogleId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.GoogleId)?.Value;
        request.AuthEmail = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Email)?.Value;

        var res = await handler.Handle(request, ct);

        if (res.Error != null) return res.Error.ToActionResult();

        return res.Value switch
        {
            AlreadyExists => Conflict(),
            Registered registered => Created(string.Empty, registered),
            _ => throw new ArgumentOutOfRangeException(nameof(res.Value))
        };
    }

    [Authorize]
    [HttpPut("resend-confirmation-email")]
    public async Task ResendConfirmationEmail([FromServices]SendEmailConfirmationCommandHandler handler, CancellationToken ct) =>
        await handler.Handle(ct);

    [AllowAnonymous]
    [HttpPut("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromServices] ConfirmEmailCommandHandler handler,
        ConfirmEmailCommand request, CancellationToken ct)
    {
        var res = await handler.Handle(request, ct);
        return res.Error != null ? res.Error.ToActionResult() : NoContent();
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromServices] UpdateUserCommandHandler handler,
        UpdateUserCommand request, CancellationToken ct)
    {
        var res = await handler.Handle(request, ct);
        return res.Error != null ? res.Error.ToActionResult() : Ok(res.Value);
    }

    [Authorize]
    [HttpPut("deactivate")]
    public async Task DeactivateAccount([FromServices] DeactivateAccountCommandHandler handle, CancellationToken ct) =>
        await handle.Handle(ct);

    [AllowAnonymous]
    [HttpPut("send-reactivate-account-email")]
    public async Task<IActionResult> SendReactivateAccountEmail(
        [FromServices] SendReactivateAccountEmailCommandHandler handler,
        SendReactivateAccountEmailCommand request, CancellationToken ct)
    {
        var res = await handler.Handle(request, ct);
        return res.Error != null ? res.Error.ToActionResult() : Ok();
    }

    [AllowAnonymous]
    [HttpPut("reactivate-account")]
    public async Task<IActionResult> ReactivateAccount([FromServices] ReactivateAccountCommandHandler handler,
        ReactivateAccountCommand request, CancellationToken ct)
    {
        var res = await handler.Handle(request, ct);
        return res.Error != null ? res.Error.ToActionResult() : NoContent();
    }

    [Authorize]
    [HttpDelete]
    public async Task DeleteAccount([FromServices] DeleteAccountCommandHandler handler, CancellationToken ct) =>
        await handler.Handle(ct);

    [Authorize]
    [HttpPost("logout-all-devices")]
    public async Task LogoutOfAllDevices([FromServices]LogoutUserOfAllDevicesCommandHandler handler, CancellationToken ct) =>
        await handler.Handle(ct);
}