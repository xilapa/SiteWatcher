namespace SiteWatcher.Domain.Users.Enums;

public enum Language
{
    BrazilianPortuguese = 1,
    English = 2,
    Spanish = 3
}

public static class LanguageUtils
{
    public const string Pt = "pt";
    public const string Es = "es";
    public static Language FromLocaleString(string? locale)
    {
        if (string.IsNullOrEmpty(locale)) return Language.English;

        var splitted = locale.ToLower().Split("-").First();

        return splitted switch
        {
            Pt => Language.BrazilianPortuguese,
            Es => Language.Spanish,
            _ => Language.English
        };
    }
}