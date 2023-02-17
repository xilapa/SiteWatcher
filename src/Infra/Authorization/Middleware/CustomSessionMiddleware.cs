using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SiteWatcher.Domain.Common.Services;
using ISession = SiteWatcher.Domain.Authentication.ISession;

namespace SiteWatcher.Infra.Authorization.Middleware;

public class CustomSessionMiddleware
{
    private readonly RequestDelegate _next;
    private ISession _session;
    private readonly ICache _cache;

    public CustomSessionMiddleware(RequestDelegate next, ICache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task Invoke(HttpContext context, ISession session)
    {
        _session = session;
        var loaded = await TryLoadSession(context);
        if (loaded)
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = 401;
    }

    private async Task<bool> TryLoadSession(HttpContext context)
    {
        // If the route allow anonymous users, then just continue
        if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            return true;

        string? userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        await _session.Load(_cache, userId);
        return true;
    }
}