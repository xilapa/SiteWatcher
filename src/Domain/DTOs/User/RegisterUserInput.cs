using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.DTOs.User;

public struct RegisterUserInput
{
    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
    public string GoogleId { get; set; }
    public string AuthEmail { get; set; }
}