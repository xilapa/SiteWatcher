using System.Security.Claims;
using System.Web;
using Domain.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Authentication.Commands.ExchangeToken;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleSettings _googleSettings;
    private readonly IMediator _mediator;
    private readonly IAppSettings _appSettings;
    private const string key = nameof(key);

    public AuthController(IAuthService authService, IGoogleSettings googleSettings, IMediator mediator,
        IAppSettings appSettings)
    {
        _authService = authService;
        _googleSettings = googleSettings;
        _mediator = mediator;
        _appSettings = appSettings;
    }

    [HttpGet]
    [Route("login")]
    [Route("register")]
    public async Task<IActionResult> StartAuth()
    {
        var state = await _authService.GenerateLoginState(_googleSettings.StateValue);
        var authUrl = $"{_googleSettings.AuthEndpoint}?" +
                      $"scope={HttpUtility.UrlEncode(_googleSettings.Scopes)}" +
                      $"&response_type=code&include_granted_scopes=false&state={state}" +
                      $"&redirect_uri={HttpUtility.UrlEncode(_googleSettings.RedirectUri)}" +
                      $"&client_id={_googleSettings.ClientId}";
        return Redirect(authUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("start/{schema:required}")]
    public IActionResult StartAuth([FromRoute] string schema)
    {
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
    [AllowAnonymous]
    [Route("google")]
    public async Task<IActionResult> GoogleAuthCallback(CancellationToken ct)
    {
        var authRes = await HttpContext.AuthenticateAsync(AuthenticationDefaults.Schemes.Google);
        if (!authRes.Succeeded) return Unauthorized();

        var authCommand = new GoogleAuthenticationCommand
        {
            GoogleId = authRes.Principal.FindFirstValue(ClaimTypes.NameIdentifier),
            ProfilePicUrl = authRes.Principal.FindFirstValue(AuthenticationDefaults.ClaimTypes.ProfilePicUrl),
            Email = authRes.Principal.FindFirstValue(ClaimTypes.Email),
            Name = authRes.Principal.FindFirstValue(ClaimTypes.Name),
            Locale = authRes.Principal.FindFirstValue(AuthenticationDefaults.ClaimTypes.Locale),
        };

        var authKeys = await _mediator.Send(authCommand, ct);
        return await SetCookieAndRedirect(authKeys);
    }

    private async Task<IActionResult> SetCookieAndRedirect(AuthKeys authKeys)
    {
        if (!authKeys.Success()) return Unauthorized(authKeys.ErrorMessage);

        // sign-in user with cookie using the key
        var claims = new[] { new Claim(key, authKeys.Key) };
        var claimsIdentity = new ClaimsIdentity(claims, AuthenticationDefaults.Schemes.Cookie);
        var cookieProps = new AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = false
        };
        var principal = new ClaimsPrincipal(claimsIdentity);
        await HttpContext.SignInAsync(AuthenticationDefaults.Schemes.Cookie, principal, cookieProps);

        // return the security token on redirect url to avoid XSRF
        var redirectUrl = $"{_appSettings.FrontEndAuthUrl}?token={authKeys.SecutriyToken}";
        return Redirect(redirectUrl);
    }

    [HttpPost]
    [Route("exchange-token")]
    public async Task<IActionResult> ExchangeToken([FromBody] ExchangeTokenCommand command, CancellationToken ct)
    {
        command.Key = HttpContext.User.Claims.FirstOrDefault(c => c.Type == key)?.Value;
        var authRes = await _mediator.Send(command, ct);
        if (authRes == null) return Unauthorized();
        return Ok(authRes);
    }
}