using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[AllowAnonymous]
[Route("google-auth")]
public class GoogleAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMediator _mediator;

    public GoogleAuthController(IAuthService authService, IMediator mediator)
    {
        _authService = authService;
        _mediator = mediator;
    }

    // [HttpGet]
    // [Route("login")]
    // [Route("register")]
    // public async Task<IActionResult> StartAuth()
    // {
    //     var state = await _authService.GenerateLoginState(_googleSettings.StateValue);
    //     var authUrl = $"{_googleSettings.AuthEndpoint}?" +
    //                   $"scope={HttpUtility.UrlEncode(_googleSettings.Scopes)}" +
    //                   $"&response_type=code&include_granted_scopes=false&state={state}" +
    //                   $"&redirect_uri={HttpUtility.UrlEncode(_googleSettings.RedirectUri)}" +
    //                   $"&client_id={_googleSettings.ClientId}";
    //     return Redirect(authUrl);
    // }

    private const string _returnUrlParameter = "returnUrl";

    [HttpGet]
    [Route("auth/{schema:required}")]
    public IActionResult StartAuth([FromRoute] string schema, [FromQuery] string returnUrl)
    {
        var callBackUrl = schema switch
        {
            AuthenticationDefaults.Schemas.Google => Url.Action(nameof(GoogleAuthCallBack)),
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(callBackUrl)) return BadRequest();

        var authProp = new AuthenticationProperties
        {
            RedirectUri = callBackUrl,
            Items = { { _returnUrlParameter, returnUrl } }
        };
        return Challenge(authProp, schema);
    }

    [HttpGet]
    [Route("google-callback")]
    public async Task<IActionResult> GoogleAuthCallBack(CancellationToken ct)
    {
        var authRes = await HttpContext.AuthenticateAsync(AuthenticationDefaults.Schemas.Google);

        // remove google auth cookie
        await HttpContext.SignOutAsync(AuthenticationDefaults.Schemas.Cookie);

        if (!authRes.Succeeded) return BadRequest();

        var locale = authRes.Principal.FindFirstValue(ClaimTypes.Locality);
        if (!string.IsNullOrEmpty(locale)) locale = locale.Split('-').First();

        var authCommand = new GoogleAuthenticationCommand {
            GoogleId = authRes.Principal.FindFirstValue(ClaimTypes.NameIdentifier),
            ProfilePicUrl = authRes.Principal.FindFirstValue(AuthenticationDefaults.ClaimTypes.ProfilePicUrl),
            Email = authRes.Principal.FindFirstValue(ClaimTypes.Email),
            Name = authRes.Principal.FindFirstValue(ClaimTypes.Name),
            Locale =  locale ?? "en",
        };

        var commandResult = await _mediator.Send(authCommand, ct);
        return Ok(commandResult);
    }
}