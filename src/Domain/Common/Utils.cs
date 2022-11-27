using System.Buffers.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SiteWatcher.Domain.Alerts.Entities.WatchModes;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Domain.Common;

public static class Utils
{
    private const int LongSize = sizeof(long);
    private const int SpanSize = sizeof(long) + 40;
    private const byte WhiteSpaceByte = (byte)' ';
    private const byte PlusByte = (byte)'+';
    private const byte SlashByte = (byte)'/';
    private const byte DefaultByte = default!;
    private const char HyphenChar = '-';
    private static readonly Dictionary<Type, WatchModes> WatchModesTypeDictionary = new()
    {
        {typeof(AnyChangesWatch), WatchModes.AnyChanges},
        {typeof(TermWatch), WatchModes.Term},
        {typeof(RegexWatch), WatchModes.Regex}
    };
    private static readonly Dictionary<char, WatchModes> WatchModesDiscriminatorDictionary = new()
    {
        {'A', WatchModes.AnyChanges},
        {'T', WatchModes.Term},
        {'R', WatchModes.Regex}
    };

    public static string GenerateSafeRandomBase64String()
    {
        // getting current time
        var currentDate = DateTime.UtcNow.Ticks;

        // allocating spans
        Span<byte> randomBytes = stackalloc byte[32 + LongSize];
        Span<byte> stateBytes = stackalloc byte[SpanSize];

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
        return ConvertBase64BytesToString(stateBytes);
    }

    internal static string ConvertBase64BytesToString(Span<byte> base64Bytes)
    {
        Span<char> chars = stackalloc char[base64Bytes.Length];

        for (var i = 0; i < base64Bytes.Length; i++)
        {
            chars[i] = base64Bytes[i] switch
            {
                WhiteSpaceByte => HyphenChar,
                PlusByte => HyphenChar,
                SlashByte => HyphenChar,
                DefaultByte => HyphenChar,
                _ => (char)base64Bytes[i]
            };
        }

        return chars.ToString();
    }

    public static string GetTokenPayload(string token)
    {
        var tokenSpan = token.AsSpan();
        var firstDotIdx = tokenSpan.IndexOf('.') + 1;
        var secondDotIdx = tokenSpan[firstDotIdx..].IndexOf('.');

        return token[firstDotIdx..(firstDotIdx + secondDotIdx)];
    }

    public static WatchModes? GetWatchModeEnumByType(WatchMode watchMode) =>
        WatchModesTypeDictionary[watchMode.GetType()];

    public static WatchModes? GetWatchModeEnumByTableDiscriminator(char discriminator) =>
        WatchModesDiscriminatorDictionary[discriminator];
}