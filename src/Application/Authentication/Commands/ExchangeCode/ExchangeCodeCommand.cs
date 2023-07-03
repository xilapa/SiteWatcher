using Mediator;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;

namespace SiteWatcher.Application.Authentication.Commands.ExchangeToken;

public sealed class ExchangeCodeCommand : ICommand<AuthenticationResult?>
{
    public string? Code { get; set; }
    public string? CodeVerifier { get; set; }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Code)) return false;
        if (string.IsNullOrEmpty(CodeVerifier)) return false;
        return true;
    }
}

public sealed class ExchangeCodeCommandHandler : ICommandHandler<ExchangeCodeCommand, AuthenticationResult?>
{
    private readonly IAuthService _authService;

    public ExchangeCodeCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async ValueTask<AuthenticationResult?> Handle(ExchangeCodeCommand request, CancellationToken ct)
    {
        if (!request.IsValid()) return null;
        return await _authService.GetAuthenticationResult(request.Code!, request.CodeVerifier!, ct);
    }
}