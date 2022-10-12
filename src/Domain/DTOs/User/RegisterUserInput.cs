using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.DTOs.User;

public class RegisterUserInput
{
    public RegisterUserInput(string name, string email, ELanguage language, ETheme theme, string googleId, string authEmail)
    {
        Name = name;
        Email = email;
        Language = language;
        Theme = theme;
        GoogleId = googleId;
        AuthEmail = authEmail;
    }

    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
    public string GoogleId { get; set; }
    public string AuthEmail { get; set; }
}