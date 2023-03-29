using SiteWatcher.Domain.Users.Enums;

namespace Domain.Authentication;

public class UserRegisterData
{
    public string GoogleId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string? Locale { get; set; }

    public Language Language() => LanguageUtils.FromLocaleString(Locale);
}