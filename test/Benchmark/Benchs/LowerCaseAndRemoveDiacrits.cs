﻿using System.Globalization;
using System.Text;
using BenchmarkDotNet.Attributes;
using SiteWatcher.Domain.Extensions;

namespace SiteWatcher.Benchmark.Benchs;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
public class LowerCaseAndRemoveDiacrits
{
    public string ToLowerCaseWithoutDiacritsOriginal(string text)
    {
        if (text.Length == 0)
            return text;

        // the normalization to FormD splits accented letters in letters+accents
        var normalizedString = text.ToLower().Normalize(NormalizationForm.FormD);

        var stringBuilder = new StringBuilder();
        foreach (var rune in normalizedString.EnumerateRunes())
        {
            var unicodeCategory = Rune.GetUnicodeCategory(rune);
            if(unicodeCategory == UnicodeCategory.NonSpacingMark)
                continue;
            stringBuilder.Append(rune);
        }

        return stringBuilder.ToString();
    }



    [Benchmark]
    public string RemoveDiacrits()
    {
        return ToLowerCaseWithoutDiacritsOriginal("Macarrão");
    }

    [Benchmark]
    public string RemoveDiacritsUsingSpan()
    {
        return "Macarrão".ToLowerCaseWithoutDiacritics();
    }
}