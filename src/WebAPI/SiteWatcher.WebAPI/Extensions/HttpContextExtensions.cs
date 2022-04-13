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

    public static string GenerateStateFromRequest(this HttpContext httpContext)
    {
        var agentBytes = JsonSerializer.SerializeToUtf8Bytes(httpContext.Request.Headers.UserAgent);
        var languageBytes = JsonSerializer.SerializeToUtf8Bytes(httpContext.Request.Headers.AcceptLanguage);
        var timeBytes = JsonSerializer.SerializeToUtf8Bytes(DateTime.UtcNow);

        var length = agentBytes.Length + languageBytes.Length + timeBytes.Length;

        var allBytes = new byte[length];
        Buffer.BlockCopy(agentBytes, 0, allBytes, 0, agentBytes.Length);
        Buffer.BlockCopy(languageBytes, 0, allBytes, 0, languageBytes.Length);
        Buffer.BlockCopy(timeBytes, 0, allBytes, 0, timeBytes.Length);

        var randGenerator = new Random();
        var start = randGenerator.Next(length / 4);
        var end = randGenerator.Next((length / 2) + 1, length);

        var truncatedBytes = new byte[end - start];
        Buffer.BlockCopy(allBytes, start, truncatedBytes, 0, end - start);

        var hashedbytes = SHA256.HashData(truncatedBytes);
        var base64State = Convert.ToBase64String(hashedbytes);
        var state = base64State.Replace(" ", "").Replace("+","");

        return state;
    }
}