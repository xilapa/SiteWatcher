using Microsoft.AspNetCore.Http;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.Extensions;

namespace SiteWatcher.Infra.Authorization;

public class Sessao : ISessao
{
    public Sessao(IHttpContextAccessor httpContextAccessor)
    {
        var claims = httpContextAccessor.HttpContext.User.Claims;
        var userIdString = claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Id)?.Value;
        Guid.TryParse(userIdString, out var userIdGuid);
        UserId = new UserId(userIdGuid);

        Email = claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Email)?.Value;
        GoogleId = claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.GoogleId)?.Value;
        UserName = claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Name)?.Value;
        AuthTokenPayload = httpContextAccessor.HttpContext.GetAuthTokenPayload();
    }

    public DateTime Now => DateTime.Now;
    public UserId? UserId { get; }
    public string? Email { get; }
    public string? GoogleId { get; }
    public string UserName { get; }
    public string AuthTokenPayload { get; }
}