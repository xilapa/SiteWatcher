using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Users.DTOs;

public class UserViewModel
{
    public UserId Id { get; set; }
    public bool Active { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public Language Language { get; set; }
    public Theme Theme { get; set; }
}