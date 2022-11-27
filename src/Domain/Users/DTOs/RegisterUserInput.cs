
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Users.DTOs;

public sealed class RegisterUserInput
{
    public RegisterUserInput(string name, string email, Language language, Theme theme, string googleId, string authEmail)
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
    public Language Language { get; set; }
    public Theme Theme { get; set; }
    public string GoogleId { get; set; }
    public string AuthEmail { get; set; }
}