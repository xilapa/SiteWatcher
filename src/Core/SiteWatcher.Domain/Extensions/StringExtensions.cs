using System.Security.Cryptography;
using System.Text;

namespace SiteWatcher.Domain.Extensions;

public static class StringExtensions
{
    public static byte[] GetHashedBytes(this string text)
    {
        var textBytes = Encoding.ASCII.GetBytes(text);
        var hashedBytes = SHA256.HashData(textBytes);
        return hashedBytes;
    }
}