using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.DTOs.User;

public struct UserViewModel
{
    private Guid Id { get; set; }
    public UserId UserId => new UserId(Id);
    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
}