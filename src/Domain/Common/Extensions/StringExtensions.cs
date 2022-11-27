using System.Buffers;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace SiteWatcher.Domain.Common.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Return the <typeparamref name="EnumT"/> equivalent value of the current string
    /// based on <typeparamref name="EnumT"/>'s Description Attributes.
    /// </summary>
    /// <typeparam name="EnumT"></typeparam>
    /// <param name="stringValue"></param>
    /// <returns></returns>
    public static EnumT? GetEnumValue<EnumT>(this string stringValue) where EnumT : Enum
    {
        var enumInstance = Activator.CreateInstance<EnumT>();
        var enumOptions = typeof(EnumT)
                            .GetFields(BindingFlags.Public | BindingFlags.Static)
                            .Select(m => new {
                                Value = m.GetValue(enumInstance),
                                DescAtt = m.GetCustomAttributes(typeof(DescriptionAttribute),false).FirstOrDefault()
                            });

        var option = enumOptions.FirstOrDefault(opts => (opts.DescAtt as DescriptionAttribute)?.Description == stringValue.ToLower());

        if (option is null)
            return default;

        return (EnumT) option.Value!;
    }

    public static string ToLowerCaseWithoutDiacritics(this string text)
    {
        if (text.Length == 0)
            return text;

        // Max length based on https://dotnet.github.io/dotNext/features/io/rental.html
        Span<char> res = text.Length > 128 ? ArrayPool<char>.Shared.Rent(text.Length) : stackalloc char[text.Length];

        // the normalization to FormD splits accented letters in letters+accents
        var normalizedString = text.Normalize(NormalizationForm.FormD).AsSpan();

        var resCounter = 0;
        var normalizedStringCounter = 0;
        foreach (var rune in normalizedString.EnumerateRunes())
        {
            var runeCategory = Rune.GetUnicodeCategory(rune);
            if (runeCategory == UnicodeCategory.NonSpacingMark)
            {
                // move to the next character on normalized string
                normalizedStringCounter++;
                continue;
            }

            res[resCounter] = runeCategory == UnicodeCategory.UppercaseLetter
                ? char.ToLower(normalizedString[normalizedStringCounter])
                : normalizedString[normalizedStringCounter];

            normalizedStringCounter++;
            resCounter++;
        }

        return res.ToString();
    }
}