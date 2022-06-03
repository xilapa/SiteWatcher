using System.Security.Claims;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Application.Interfaces;

public interface IAuthService
{
    string GenerateLoginToken(UserViewModel user);
    string GenerateLoginToken(Domain.Models.User user);
    string GenerateRegisterToken(IEnumerable<Claim> tokenClaims, string googleId);

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

    Task<UserId?> GetUserIdFromEmailConfirmationToken(string token);
}