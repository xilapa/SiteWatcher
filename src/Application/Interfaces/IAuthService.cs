using System.Security.Claims;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Domain.ViewModels;

namespace SiteWatcher.Application.Interfaces;

public interface IAuthService
{
    string GenerateLoginToken(UserViewModel user);
    string GenerateLoginToken(Domain.Models.User user);
    string GenerateRegisterToken(IEnumerable<Claim> tokenClaims, string googleId);

    /// <summary>
    /// Invalidate the login token of the current user
    /// </summary>
    /// <param name="invalidateUser">Also invalidated the current user</param>
    Task InvalidateCurrenUser();

    Task InvalidateCurrentRegisterToken();
    Task<bool> IsRegisterTokenValid();
    Task<bool> UserCanLogin();
    Task<string> GenerateLoginState(byte[] stateBytes);
    Task WhiteListToken(UserId userId, string token);
}