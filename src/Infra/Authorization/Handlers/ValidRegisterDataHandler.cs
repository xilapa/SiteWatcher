using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.Extensions;

namespace SiteWatcher.Infra.Authorization.Handlers;

public class ValidRegisterDataHandler : AuthorizationHandler<ValidRegisterData>
{
    private readonly IAuthService _authService;
    private readonly HttpContext? _httpContext;

    public ValidRegisterDataHandler(IAuthService authService, IHttpContextAccessor httpContext)
    {
        _authService = authService;
        _httpContext = httpContext.HttpContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        ValidRegisterData requirement)
    {
        var authTokenPayload = _httpContext!.GetAuthTokenPayload();

        var registerTokenValid = await _authService.IsRegisterTokenValid(authTokenPayload);
        if (registerTokenValid)
        {
            context.Succeed(requirement);
            _httpContext!.Items.Add(AuthenticationDefaults.AuthtokePayloadKey, authTokenPayload);
        }
    }
}