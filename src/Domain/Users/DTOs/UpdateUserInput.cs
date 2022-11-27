using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Users.DTOs;

public class UpdateUserInput
{
    public UpdateUserInput(string name, string email, Language language, Theme theme)
    {
        Name = name;
        Email = email;
        Language = language;
        Theme = theme;
    }

    public UpdateUserInput()
    { }

    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Language Language { get; set; }
    public Theme Theme { get; set; }
}