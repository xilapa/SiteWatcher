using System.Buffers.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Users.Enums;

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
    private static readonly Dictionary<Type, Rules> RulesTypeDictionary = new()
    {
        {typeof(AnyChangesRule), Rules.AnyChanges},
        {typeof(TermRule), Rules.Term},
        {typeof(RegexRule), Rules.Regex}
    };
    private static readonly Dictionary<char, Rules> RulesDiscriminatorDictionary = new()
    {
        {'A', Rules.AnyChanges},
        {'T', Rules.Term},
        {'R', Rules.Regex}
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

    public static Rules? GetRuleEnumByType(Rule rule) =>
        RulesTypeDictionary[rule.GetType()];

    public static Rules? GetRuleEnumByTableDiscriminator(char discriminator) =>
        RulesDiscriminatorDictionary[discriminator];

    public static Language GetLanguageFromLocale(string locale)
    {
        locale = locale.Split('-')[0].ToLower();
        return locale switch {
            "en" => Language.English,
            "es" => Language.Spanish,
            "pt" => Language.BrazilianPortuguese,
            _ => Language.English
        };
    }
}