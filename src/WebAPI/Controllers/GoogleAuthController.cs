using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application.Authentication.Commands.Authenticate;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.WebAPI.Controllers;

[ApiController]
[Route("google-auth")]
public class GoogleAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMediator _mediator;
    private readonly IAppSettings _appSettings;
    private readonly ITimeLimitedDataProtector _protector;
    private const string _returnUrlParameter = "returnUrl";

    public GoogleAuthController(IAuthService authService, IMediator mediator, IAppSettings appSettings, IDataProtectionProvider protectionProvider)
    {
        _authService = authService;
        _mediator = mediator;
        _appSettings = appSettings;
        _protector = protectionProvider.CreateProtector(nameof(GoogleAuthController)).ToTimeLimitedDataProtector();
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("auth/{schema:required}")]
    public IActionResult StartAuth([FromRoute] string schema)
    {
        var callBackUrl = schema switch
        {
            AuthenticationDefaults.Schemas.Google => Url.Action(nameof(GoogleAuthCallBack)),
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(callBackUrl)) return BadRequest();

        var authProp = new AuthenticationProperties
        {
            RedirectUri = callBackUrl
        };
        return Challenge(authProp, schema);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("google-callback")]
    public async Task<IActionResult> GoogleAuthCallBack(CancellationToken ct)
    {
        var authRes = await HttpContext.AuthenticateAsync(AuthenticationDefaults.Schemas.Cookie);

        if (!authRes.Succeeded) return BadRequest();

        var authCommand = new GoogleAuthenticationCommand
        {
            GoogleId = authRes.Principal.FindFirstValue(ClaimTypes.NameIdentifier),
            ProfilePicUrl = authRes.Principal.FindFirstValue(AuthenticationDefaults.ClaimTypes.ProfilePicUrl),
            Email = authRes.Principal.FindFirstValue(ClaimTypes.Email),
            Name = authRes.Principal.FindFirstValue(ClaimTypes.Name),
            Locale = authRes.Principal.FindFirstValue(ClaimTypes.Locality),
        };

        var commandResult = await _mediator.Send(authCommand, ct);

        if (!commandResult.Success)
            return BadRequest(commandResult.Error);

        // signin user with the auth session token
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, commandResult.Token) };
        var claimsIdentity = new ClaimsIdentity(claims, AuthenticationDefaults.Schemas.Cookie);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = false,
            // ExpiresUtc = DateTime.UtcNow.AddSeconds(30)
        };
        await HttpContext.SignInAsync(AuthenticationDefaults.Schemas.Cookie,
            new ClaimsPrincipal(claimsIdentity),
            authProps);

        var protectedToken = _protector
            .Protect(commandResult.Token, TimeSpan.FromSeconds(30));

        var base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(protectedToken));

        return Redirect(_appSettings.FrontEndUrl + "/?token=" + base64Token);
    }

    [HttpPost]
    [Authorize]
    [Route("session")]
    public async Task<IActionResult> CreateSession([FromBody] AuthenticateCommand command)
    {
        var bytes = Convert.FromBase64String(command.Token);
        var token = Encoding.UTF8.GetString(bytes);
        try
        {
            command.Token = _protector.Unprotect(token);
        }
        catch
        {
            return BadRequest();
        }

        if (string.IsNullOrEmpty(command.Token)) return BadRequest();
        var res = await _mediator.Send(command);

        // signin user with the auth session token
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, res.UserId) };
        var claimsIdentity = new ClaimsIdentity(claims, AuthenticationDefaults.Schemas.Cookie);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = false,
            // ExpiresUtc = DateTime.UtcNow.AddSeconds(30)
        };
        await HttpContext.SignInAsync(AuthenticationDefaults.Schemas.Cookie,
            new ClaimsPrincipal(claimsIdentity),
            authProps);

        res.UserId = null;
        return Ok(res);
    }
}