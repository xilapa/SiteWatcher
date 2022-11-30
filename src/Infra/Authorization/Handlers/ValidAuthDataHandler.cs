using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SiteWatcher.Common.Services;

namespace SiteWatcher.Infra.Authorization.Handlers;

public class ValidAuthDataHandler : AuthorizationHandler<ValidAuthData>
{
    private readonly IAuthService _authService;

    public ValidAuthDataHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidAuthData requirement)
    {
        if (context is null)
            return;

        var canLogin = await _authService.UserCanLogin();
        if(canLogin)
            context.Succeed(requirement);
    }
}