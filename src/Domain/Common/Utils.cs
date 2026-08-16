using System.Security.Cryptography;

namespace SiteWatcher.Domain.Common;

public static class Utils
{
    public static string GenerateSafeRandomBase64String()
    {
        Span<byte> randomBytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(randomBytes);

        // TD: Upgrade to .Net 9 or later to use Base64Url.EncodeToString
        // return Base64Url.EncodeToString(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static string GetTokenPayload(string token)
    {
        var tokenSpan = token.AsSpan();
        var firstDotIdx = tokenSpan.IndexOf('.') + 1;
        var secondDotIdx = tokenSpan[firstDotIdx..].IndexOf('.');

        return token[firstDotIdx..(firstDotIdx + secondDotIdx)];
    }
}