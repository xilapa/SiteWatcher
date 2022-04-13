using System.Security.Claims;

namespace SiteWatcher.WebAPI.Extensions;

public static class ClaimsExtensions
{
    public static Claim GetClaimValue(this IEnumerable<Claim> claims, string type) 
    {
        var claimValue = claims.DefaultIfEmpty(new Claim(type, string.Empty))
                                    .FirstOrDefault(c => c.Type == type)!.Value;

        var claim = new Claim(type, claimValue);
        return claim;
    }
}