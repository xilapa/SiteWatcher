using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Domain.Common.Services;

public interface IAuthService
{
    string GenerateRegisterToken(string googleId, string name, string email, string locale);

    /// <summary>
    /// Invalidate the current user
    /// </summary>
    Task InvalidateCurrenUser();

    Task InvalidateCurrentRegisterToken();
    Task<bool> IsRegisterTokenValid();
    Task<bool> UserCanLogin();
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

    Task<string> CreateLoginAuthSession(UserViewModel user, string? profilePictureUrl = null);
    Task<string> CreateRegisterAuthSession(string googleId, string name, string email, string locale, string? profilePictureUrl = null);
    Task<string> CreateActivateAuthSession(UserViewModel user);

    Task<SessionView> GenerateSession(string token);
}