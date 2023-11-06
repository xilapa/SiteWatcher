using Mediator;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;

namespace SiteWatcher.Application.Users.Commands.LogoutUserOfAllDevices;

public class LogoutUserOfAllDevicesCommand : ICommand
{ }

public class LogoutUserOfAllDevicesCommandHandler : ICommandHandler<LogoutUserOfAllDevicesCommand>
{
    private readonly IAuthService _authService;
    private readonly ISession _session;

    public LogoutUserOfAllDevicesCommandHandler(IAuthService authService, ISession session)
    {
        _authService = authService;
        _session = session;
    }

    public async ValueTask<Unit> Handle(LogoutUserOfAllDevicesCommand request, CancellationToken cancellationToken)
    {
        await _authService.InvalidateCurrenUser(_session);
        return Unit.Value;
    }
}
