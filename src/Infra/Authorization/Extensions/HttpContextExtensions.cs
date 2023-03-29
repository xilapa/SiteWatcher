using Microsoft.AspNetCore.Http;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Infra.Authorization.Extensions;

public static class HttpContextExtensions
{
    public static string GetAuthTokenPayload(this HttpContext httpContext)
    {
        var token = httpContext.Request.Headers.Authorization;
        if(string.IsNullOrEmpty(token))
            return string.Empty;

        var tokenString = token.ToString();
        return Utils.GetTokenPayload(tokenString);
    }

    public static T? GetItem<T>(this HttpContext httpContext, string key)
    {
        httpContext.Items.TryGetValue(key, out var itemObj);
        if (itemObj is T typed) return typed;
        return default;
    }
}