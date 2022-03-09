using System.Collections.Generic;
using System.Security.Claims;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models;

namespace SiteWatcher.Domain.Interfaces;

public interface ITokenService
{
    string GenerateUserToken(ETokenPurpose purpose, User user);
    string GenerateRegisterToken(IEnumerable<Claim> tokenClaims, string googleId);
}