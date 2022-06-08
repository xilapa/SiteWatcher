using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.Extensions;
using ISession = SiteWatcher.Application.Interfaces.ISession;

namespace SiteWatcher.Infra.Authorization;

public class Session : ISession
{
    public Session(IHttpContextAccessor httpContextAccessor)
    {
        var claims = httpContextAccessor.HttpContext.User.Claims;
        var claimsEnumerated = claims as Claim[] ?? claims.ToArray();
        var userIdString = Array.Find(claimsEnumerated, c => c.Type == AuthenticationDefaults.ClaimTypes.Id)?.Value;
        Guid.TryParse(userIdString, out var userIdGuid);
        UserId = new UserId(userIdGuid);

        Email = Array.Find(claimsEnumerated, c => c.Type == AuthenticationDefaults.ClaimTypes.Email)?.Value;
        GoogleId = Array.Find(claimsEnumerated, c => c.Type == AuthenticationDefaults.ClaimTypes.GoogleId)?.Value;
        UserName = Array.Find(claimsEnumerated, c => c.Type == AuthenticationDefaults.ClaimTypes.Name)?.Value;

        var hasLang =
            int.TryParse(Array.Find(claimsEnumerated, c => c.Type == AuthenticationDefaults.ClaimTypes.Language)?.Value,
                out var lang);
        Language = hasLang ? (ELanguage) lang : null;

        var authenticated = httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
        AuthTokenPayload = authenticated ? httpContextAccessor.HttpContext.GetAuthTokenPayload() : string.Empty;
    }

    // Get the utc date without timezone
    public DateTime Now => new(DateTime.UtcNow.Ticks);
    public UserId? UserId { get; }
    public string? Email { get; }
    public string? GoogleId { get; }
    public string? UserName { get; }
    public ELanguage? Language { get; }
    public string AuthTokenPayload { get; }
}