using System.Buffers.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SiteWatcher.Domain.Utils;

public static class Utils
{
    private const int LongSize = sizeof(long);
    private const int SpanSize = sizeof(long) + 40;
    private const byte WhiteSpaceByte = (byte) ' ';
    private const byte PlusByte = (byte) '+';
    private const byte SlashByte = (byte) '/';
    private const byte DefaultByte = default!;
    private const char HyphenChar = '-';

    public static string GenerateSafeRandomBase64String()
    {
        // getting current time
        var currentDate = DateTime.UtcNow.Ticks;

        // allocating spans
        Span<byte> randomBytes = stackalloc byte[32 + LongSize];
        Span<byte> stateBytes = stackalloc byte[SpanSize];
        Span<char> stateChars = stackalloc char[SpanSize];

        // generating a secure random number
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes[..32]);
        }

        // copying current time bytes
        MemoryMarshal.Write(randomBytes[32..], ref currentDate);

        // converting the random bytes to base64 bytes
        Base64.EncodeToUtf8(randomBytes, stateBytes, out _, out _);

        // converting base64 bytes to chars
        for (var i = 0; i < SpanSize; i++)
        {
            stateChars[i] = stateBytes[i] switch
            {
                WhiteSpaceByte => HyphenChar,
                PlusByte => HyphenChar,
                SlashByte => HyphenChar,
                DefaultByte => HyphenChar,
                _ => (char) stateBytes[i]
            };
        }

        return stateChars.ToString();
    }
}