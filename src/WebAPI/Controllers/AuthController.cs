using System.Security.Claims;
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
    private readonly IAppSettings _appSettings;

    public AuthController(IAppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    [HttpGet]
    [Route("start/{schema:required}")]
    public IActionResult StartAuth([FromRoute] string schema, [FromQuery] string codeChallenge)
    {
        var callBackUrl = schema switch
        {
            AuthenticationDefaults.Schemes.Google => Url.Action(nameof(GoogleAuthCallback)),
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(callBackUrl)) return BadRequest();

        var authProp = new AuthenticationProperties { RedirectUri = callBackUrl };
        authProp.Items.Add(AuthenticationDefaults.CodeChallengeKey, codeChallenge);
        return Challenge(authProp, schema);
    }

    [HttpGet]
    [Route("google")]
    public async Task<IActionResult> GoogleAuthCallback([FromServices] AuthenticationCommandHandler handler,
        CancellationToken ct)
    {
        var authRes = await HttpContext.AuthenticateAsync(AuthenticationDefaults.Schemes.Google);
        if (!authRes.Succeeded) return Unauthorized();

        authRes.Properties.Items.TryGetValue(AuthenticationDefaults.CodeChallengeKey, out var codeChallenge);

        var authCommand = new AuthenticationCommand
        {
            GoogleId = authRes.Principal.FindFirstValue(ClaimTypes.NameIdentifier),
            ProfilePicUrl = authRes.Principal.FindFirstValue(AuthenticationDefaults.ClaimTypes.ProfilePicUrl),
            Email = authRes.Principal.FindFirstValue(ClaimTypes.Email),
            Name = authRes.Principal.FindFirstValue(ClaimTypes.Name),
            Locale = authRes.Principal.FindFirstValue(AuthenticationDefaults.ClaimTypes.Locale),
            CodeChallenge = codeChallenge ?? string.Empty
        };

        var authCodeRes = await handler.Handle(authCommand, ct);
        if (!authCodeRes.Success()) return Unauthorized();
        var redirectUrl = $"{_appSettings.FrontEndAuthUrl}?code={authCodeRes.Code}";
        return Redirect(redirectUrl);
    }

    [HttpPost]
    [Route("exchange-code")]
    public async Task<IActionResult> ExchangeCode([FromServices] ExchangeCodeCommandHandler handler,
        [FromBody] ExchangeCodeCommand request, CancellationToken ct)
    {
        await HttpContext.SignOutAsync(AuthenticationDefaults.Schemes.Cookie);
        var authRes = await handler.Handle(request, ct);
        if (authRes == null) return Unauthorized();
        return Ok(authRes);
    }
}