using MediatR;
using SiteWatcher.Common.Services;

namespace SiteWatcher.Application.Users.Commands.LogoutUserOfAllDevices;

public class LogoutUserOfAllDevicesCommand : IRequest
{ }

public class LogoutUserOfAllDevicesCommandHandler : IRequestHandler<LogoutUserOfAllDevicesCommand>
{
    private readonly IAuthService _authService;

    public LogoutUserOfAllDevicesCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task Handle(LogoutUserOfAllDevicesCommand request, CancellationToken cancellationToken) =>
        _authService.InvalidateCurrenUser();
}
