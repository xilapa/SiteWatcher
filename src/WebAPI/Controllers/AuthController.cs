using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Authentication.Commands.Authentication;
using SiteWatcher.Application.Authentication.Commands.ExchangeToken;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[AllowAnonymous]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppSettings _appSettings;
    private const string key = nameof(key);

    public AuthController(IMediator mediator, IAppSettings appSettings)
    {
        _mediator = mediator;
        _appSettings = appSettings;
    }

    [HttpGet]
    [Route("start/{schema:required}")]
    public IActionResult StartAuth([FromRoute] string schema)
    {
        // TODO: receive code_challenge
        var callBackUrl = schema switch
        {
            AuthenticationDefaults.Schemes.Google => Url.Action(nameof(GoogleAuthCallback)),
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(callBackUrl)) return BadRequest();

        var authProp = new AuthenticationProperties { RedirectUri = callBackUrl };
        return Challenge(authProp, schema);
    }

    [HttpGet]
    [Route("google")]
    public async Task<IActionResult> GoogleAuthCallback(CancellationToken ct)
    {
        var authRes = await HttpContext.AuthenticateAsync(AuthenticationDefaults.Schemes.Google);
        if (!authRes.Succeeded) return Unauthorized();

        var authCommand = new AuthenticationCommand
        {
            GoogleId = authRes.Principal.FindFirstValue(ClaimTypes.NameIdentifier),
            ProfilePicUrl = authRes.Principal.FindFirstValue(AuthenticationDefaults.ClaimTypes.ProfilePicUrl),
            Email = authRes.Principal.FindFirstValue(ClaimTypes.Email),
            Name = authRes.Principal.FindFirstValue(ClaimTypes.Name),
            Locale = authRes.Principal.FindFirstValue(AuthenticationDefaults.ClaimTypes.Locale)
        };

        var authCodeRes = await _mediator.Send(authCommand, ct);
        var redirectUrl = $"{_appSettings.FrontEndAuthUrl}?code={authCodeRes.Code}";
        return Redirect(redirectUrl);
    }

    [HttpPost]
    [Route("exchange-code")]
    public async Task<IActionResult> ExchangeCode([FromBody] ExchangeCodeCommand command, CancellationToken ct)
    {
        // TODO: receive code_verifier
        await HttpContext.SignOutAsync(AuthenticationDefaults.Schemes.Cookie);
        var authRes = await _mediator.Send(command, ct);
        if (authRes == null) return Unauthorized();
        return Ok(authRes);
    }
}