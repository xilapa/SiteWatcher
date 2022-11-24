using System.Security.Claims;

namespace SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;

public class GoogleTokenResult
{
    public GoogleTokenResult(string googleId, string? profilePicUrl, IEnumerable<Claim> claims)
    {
        GoogleId = googleId;
        ProfilePicUrl = profilePicUrl;
        Claims = claims;
        Success = true;
    }

    public GoogleTokenResult(bool success)
    {
        Success = success;
        Claims = Enumerable.Empty<Claim>();
    }

    public string? GoogleId { get; }
    public string? ProfilePicUrl { get; }
    public bool Success { get; }
    public IEnumerable<Claim> Claims { get; }
}