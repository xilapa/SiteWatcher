using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.Extensions;
using ISession = SiteWatcher.Domain.Authentication.ISession;

namespace SiteWatcher.Infra.Authorization;

// ISession can't be injected on ctor because IAuthService is used on Authz Handlers.
// When AuthService is constructed to handle authorization, the HttpContext used to
// construct Session doesn't have the User logged.
// Session needs to be constructed after Authz Handlers.
public class Session : ISession
{
    public Session(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        // try get the value from httpcontext items
        var claims = httpContext?.GetItem<Claim[]>(AuthenticationDefaults.ClaimsKey) ??
                     httpContextAccessor.HttpContext?.User.Claims.ToArray();

        if (claims == null)
        {
            AuthTokenPayload = string.Empty;
            return;
        }

        // try get user if from httpcontext items
        var userId = httpContext?.GetItem<UserId>(AuthenticationDefaults.UserIdKey);

        if (userId == null)
        {
            var userIdString = Array.Find(claims, c => c.Type == AuthenticationDefaults.ClaimTypes.Id)?.Value;
            Guid.TryParse(userIdString, out var userIdGuid);
            UserId = new UserId(userIdGuid);
        }

        UserId = userId;

        Email = Array.Find(claims, c => c.Type == AuthenticationDefaults.ClaimTypes.Email)?.Value;
        GoogleId = Array.Find(claims, c => c.Type == AuthenticationDefaults.ClaimTypes.GoogleId)?.Value;
        UserName = Array.Find(claims, c => c.Type == AuthenticationDefaults.ClaimTypes.Name)?.Value;

        var intLang = Array.Find(claims, c => c.Type == AuthenticationDefaults.ClaimTypes.Language)?.Value;
        var hasLang = int.TryParse(intLang, out var lang);
        Language = hasLang ? (Language)lang : null;

        var authenticated = httpContext!.User.Identity!.IsAuthenticated;
        if (!authenticated)
        {
            AuthTokenPayload = string.Empty;
            return;
        }

        AuthTokenPayload = httpContext.GetItem<string>(AuthenticationDefaults.AuthtokenPayloadKey) ??
                           httpContext.GetAuthTokenPayload();
    }

    public Session()
    {
        AuthTokenPayload = string.Empty;
    }

    public virtual DateTime Now => DateTime.UtcNow;
    public UserId? UserId { get; }
    public string? Email { get; }
    public string? GoogleId { get; }
    public string? UserName { get; }
    public Language? Language { get; }
    public string AuthTokenPayload { get; }
}