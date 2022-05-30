using MediatR;
using SiteWatcher.Application.Interfaces;

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

    public async Task<Unit> Handle(LogoutUserOfAllDevicesCommand request, CancellationToken cancellationToken)
    {
        // invalidate current token and user
        await _authService.InvalidateCurrenUser();

        return Unit.Value;
    }
}
