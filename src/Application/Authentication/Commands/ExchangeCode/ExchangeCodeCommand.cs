using MediatR;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;

namespace SiteWatcher.Application.Authentication.Commands.ExchangeToken;

public sealed class ExchangeCodeCommand : IRequest<AuthenticationResult?>
{
    public string? Code { get; set; }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Code)) return false;
        return true;
    }
}

public sealed class ExchangeCodeCommandHandler : IRequestHandler<ExchangeCodeCommand, AuthenticationResult?>
{
    private readonly IAuthService _authService;

    public ExchangeCodeCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthenticationResult?> Handle(ExchangeCodeCommand request, CancellationToken ct)
    {
        if (!request.IsValid()) return null;
        return await _authService.GetAuthenticationResult(request.Code!, ct);
    }
}