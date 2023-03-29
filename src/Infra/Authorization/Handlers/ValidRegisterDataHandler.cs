using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Infra.Authorization.Extensions;

namespace SiteWatcher.Infra.Authorization.Handlers;

public class ValidRegisterDataHandler : AuthorizationHandler<ValidRegisterData>
{
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _httpContext;

    public ValidRegisterDataHandler(IAuthService authService, IHttpContextAccessor httpContext)
    {
        _authService = authService;
        _httpContext = httpContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        ValidRegisterData requirement)
    {
        var authTokenPayload = _httpContext.HttpContext!.GetAuthTokenPayload();

        var registerTokenValid = await _authService.IsRegisterTokenValid(authTokenPayload);
        if (registerTokenValid)
            context.Succeed(requirement);

        // todo: store authTokenPayload on httpContext items, so session can use the value
    }
}