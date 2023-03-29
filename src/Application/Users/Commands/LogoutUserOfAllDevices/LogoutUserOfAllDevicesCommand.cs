using MediatR;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;

namespace SiteWatcher.Application.Users.Commands.LogoutUserOfAllDevices;

public class LogoutUserOfAllDevicesCommand : IRequest
{ }

public class LogoutUserOfAllDevicesCommandHandler : IRequestHandler<LogoutUserOfAllDevicesCommand>
{
    private readonly IAuthService _authService;
    private readonly ISession _session;

    public LogoutUserOfAllDevicesCommandHandler(IAuthService authService, ISession session)
    {
        _authService = authService;
        _session = session;
    }

    public Task Handle(LogoutUserOfAllDevicesCommand request, CancellationToken cancellationToken) =>
        _authService.InvalidateCurrenUser(_session);
}
