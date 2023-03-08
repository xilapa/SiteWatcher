using Domain.Authentication;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Common.Services;

public interface IAuthService
{
    string GenerateLoginToken(UserViewModel user);
    string GenerateLoginToken(User user);
    string GenerateRegisterToken(UserRegisterData user);

    /// <summary>
    /// Invalidate the current user
    /// </summary>
    Task InvalidateCurrenUser();

    Task InvalidateCurrentRegisterToken();
    Task<bool> IsRegisterTokenValid();
    Task<bool> UserCanLogin();
    Task<string> GenerateLoginState(byte[] stateBytes);
    Task WhiteListToken(UserId userId, string token);
    Task WhiteListTokenForCurrentUser(string token);

    /// <summary>
    /// The token is saved on redis as key, and the user id is saved as value.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<string> SetEmailConfirmationTokenExpiration(string token, UserId userId);

    Task<UserId?> GetUserIdFromConfirmationToken(string token);

    /// <summary>
    /// The token is saved on redis as key, and the user id is saved as value.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<string> SetAccountActivationTokenExpiration(string token, UserId userId);

    Task<AuthKeys> StoreAuthenticationResult(AuthenticationResult authRes, CancellationToken ct);
}