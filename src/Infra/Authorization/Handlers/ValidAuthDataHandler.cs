using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.Extensions;

namespace SiteWatcher.Infra.Authorization.Handlers;

public class ValidAuthDataHandler : AuthorizationHandler<ValidAuthData>
{
    private readonly IAuthService _authService;
    private readonly HttpContext? _httpContext;

    public ValidAuthDataHandler(IAuthService authService, IHttpContextAccessor httpContext)
    {
        _authService = authService;
        _httpContext = httpContext.HttpContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidAuthData requirement)
    {
        var claims = _httpContext?.User.Claims;
        if (claims == null) return;

        var claimsEnumerated = claims as Claim[] ?? claims.ToArray();
        var userIdString = Array.Find(claimsEnumerated, c => c.Type == AuthenticationDefaults.ClaimTypes.Id)?.Value;
        var validId = Guid.TryParse(userIdString, out var userIdGuid);
        if (!validId) return;

        var authTokenPayload = _httpContext!.GetAuthTokenPayload();

        var canLogin = await _authService.UserCanLogin(new UserId(userIdGuid), authTokenPayload);
        if (canLogin)
        {
            context.Succeed(requirement);
            _httpContext!.Items.Add(AuthenticationDefaults.AuthtokePayloadKey, authTokenPayload);
            _httpContext!.Items.Add(AuthenticationDefaults.UserIdKey, validId);
            _httpContext!.Items.Add(AuthenticationDefaults.ClaimsKey, claimsEnumerated);
        }
    }
}