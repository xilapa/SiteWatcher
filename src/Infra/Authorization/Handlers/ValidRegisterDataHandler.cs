using Microsoft.AspNetCore.Authorization;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Infra.Authorization.Handlers;

public class ValidRegisterDataHandler : AuthorizationHandler<ValidRegisterData>
{
    private readonly IAuthService _authService;

    public ValidRegisterDataHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidRegisterData requirement)
    {
        if (context is null)
            return;

        var registerTokenValid = await _authService.IsRegisterTokenValid();
        if(registerTokenValid)
            context.Succeed(requirement);
    }
}