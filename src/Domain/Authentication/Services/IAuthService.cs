using Domain.Authentication;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Domain.Authentication.Services;

public interface IAuthService
{
    string GenerateLoginToken(UserViewModel user);
    string GenerateLoginToken(User user);
    string GenerateRegisterToken(UserRegisterData user);

    /// <summary>
    /// Invalidate the current user
    /// </summary>
    Task InvalidateCurrenUser(ISession session);

    Task InvalidateCurrentRegisterToken(ISession session);
    Task<bool> IsRegisterTokenValid(string authTokenPayload);
    Task<bool> UserCanLogin(UserId? userId, string authTokenPayload);
    Task<string> GenerateLoginState(byte[] stateBytes);
    Task WhiteListToken(UserId userId, string token);
    Task WhiteListTokenForCurrentUser(ISession session, string token);

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

    Task<AuthCodeResult> StoreAuthenticationResult(AuthenticationResult authRes, CancellationToken ct);
    Task<AuthenticationResult?> GetAuthenticationResult(string code, CancellationToken ct);
}