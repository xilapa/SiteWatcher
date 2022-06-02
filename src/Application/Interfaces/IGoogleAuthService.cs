using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;

namespace SiteWatcher.Application.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleTokenResult> ExchangeCode(string code, CancellationToken cancellationToken);
}