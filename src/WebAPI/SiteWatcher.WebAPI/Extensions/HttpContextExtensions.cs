using System.Security.Cryptography;
using System.Text.Json;

namespace SiteWatcher.WebAPI.Extensions;

public static class HttpContextExtensions
{
    public static string GetAuthTokenPayload(this HttpContext httpContext)
    {
        var token = httpContext.Request.Headers.Authorization;
        if(string.IsNullOrEmpty(token))
            return string.Empty;

        var tokenString = token.ToString();
        var tokenSpan = tokenString.AsSpan();
        var firstDotIdx = tokenSpan.IndexOf('.') + 1;
        var secondDotIdx = tokenSpan[firstDotIdx..].IndexOf('.');

        return tokenString[firstDotIdx .. secondDotIdx];
    }
}