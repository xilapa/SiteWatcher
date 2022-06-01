using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.DTOs.User;

public struct UpdateUserInput
{
    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
}