using System.Buffers;
using System.Globalization;
using System.Text;

namespace SiteWatcher.Domain.Common.Extensions;

public static class StringExtensions
{
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