using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.DTOs.User;

public class UserViewModel
{
    public UserId Id { get; set; }
    public bool Active { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public ELanguage Language { get; set; }
    public ETheme Theme { get; set; }
}