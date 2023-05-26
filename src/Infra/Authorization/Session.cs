using Microsoft.AspNetCore.Http;
using SiteWatcher.Domain.Common.ValueObjects;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Session(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual DateTime Now => DateTime.UtcNow;

    private UserId? _userId;
    public UserId? UserId => GetUserId();

    private UserId GetUserId()
    {
        if (_userId != null) return _userId.Value;
        var httpContext = _httpContextAccessor.HttpContext;

        // try get user from httpcontext items
        _userId = httpContext?.GetItem<UserId>(AuthenticationDefaults.UserIdKey);
        if (_userId != null) return _userId.Value;

        var userIdString = httpContext?.User.Claims
            .FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Id)?.Value;
        Guid.TryParse(userIdString, out var userIdGuid);
        _userId = new UserId(userIdGuid);
        return _userId.Value;
    }

    public string AuthTokenPayload =>
        _httpContextAccessor.HttpContext?.GetItem<string>(AuthenticationDefaults.AuthTokenPayloadKey) ??
        _httpContextAccessor.HttpContext?.GetAuthTokenPayload() ??
        string.Empty;
}