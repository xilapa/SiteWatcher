using MediatR;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Application.Authentication.Commands.Authenticate;

public sealed class AuthenticateCommand : IRequest<SessionView>
{
    public string Token { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Token);
    }
}

public sealed class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, SessionView>
{
    private readonly IAuthService _authService;

    public AuthenticateCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<SessionView> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        return await _authService.GenerateSession(request.Token);
    }
}