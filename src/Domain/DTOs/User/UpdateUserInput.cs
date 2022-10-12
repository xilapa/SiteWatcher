using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.DTOs.User;

public class UpdateUserInput
{
    public UpdateUserInput(string name, string email, ELanguage language, ETheme theme)
    {
        Name = name;
        Email = email;
        Language = language;
        Theme = theme;
    }

    public UpdateUserInput()
    { }

    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
}