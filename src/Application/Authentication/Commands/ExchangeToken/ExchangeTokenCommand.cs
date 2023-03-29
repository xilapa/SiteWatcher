using MediatR;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;

namespace SiteWatcher.Application.Authentication.Commands.ExchangeToken;

public sealed class ExchangeTokenCommand : IRequest<AuthenticationResult?>
{
    public string? Key { get; set; }
    public string? Token { get; set; }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Key)) return false;
        if (string.IsNullOrEmpty(Token)) return false;
        return true;
    }
}

public sealed class ExchangeTokenCommandHandler : IRequestHandler<ExchangeTokenCommand, AuthenticationResult?>
{
    private readonly IAuthService _authService;

    public ExchangeTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthenticationResult?> Handle(ExchangeTokenCommand request, CancellationToken ct)
    {
        if (!request.IsValid()) return null;
        return await _authService.GetAuthenticationResult(request.Key!, request.Token!, ct);
    }
}