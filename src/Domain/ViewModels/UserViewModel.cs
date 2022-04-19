using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.ViewModels;

public struct UserViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public ELanguage Language { get; set; }
}