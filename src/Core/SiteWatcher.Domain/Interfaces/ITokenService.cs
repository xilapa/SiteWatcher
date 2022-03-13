using System.Security.Claims;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.ViewModels;

namespace SiteWatcher.Domain.Interfaces;

public interface ITokenService
{
    string GenerateLoginToken(UserViewModel user);
    string GenerateLoginToken(User user);
    string GenerateRegisterToken(IEnumerable<Claim> tokenClaims, string googleId);
    Task InvalidateToken(string token, ETokenPurpose tokenPurpose);
    Task<bool> IsValid(string token);
}