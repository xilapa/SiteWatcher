using SiteWatcher.Application.Common.Command;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;

namespace SiteWatcher.Application.Users.Commands.LogoutUserOfAllDevices;

public class LogoutUserOfAllDevicesCommandHandler : IApplicationHandler
{
    private readonly IAuthService _authService;
    private readonly ISession _session;

    public LogoutUserOfAllDevicesCommandHandler(IAuthService authService, ISession session)
    {
        _authService = authService;
        _session = session;
    }

    public async Task Handle(CancellationToken cancellationToken) =>
        await _authService.InvalidateCurrenUser(_session);
}
