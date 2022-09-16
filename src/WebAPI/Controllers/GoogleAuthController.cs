using System.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Authentication.Common;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI.Filters;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[AllowAnonymous]
[Route("google-auth")]
public class GoogleAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleSettings _googleSettings;
    private readonly IMediator _mediator;

    public GoogleAuthController(IAuthService authService, IGoogleSettings googleSettings, IMediator mediator)
    {
        _authService = authService;
        _googleSettings = googleSettings;
        _mediator = mediator;
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

    [CommandValidationFilter]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate(GoogleAuthenticationCommand command, CancellationToken cancellationToken)
    {
        var commandResult = await _mediator.Send(command, cancellationToken);
        return commandResult.Handle<AuthenticationResult>();
    }
}