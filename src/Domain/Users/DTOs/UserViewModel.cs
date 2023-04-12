using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Users.DTOs;

public class UserViewModel
{
    public UserViewModel()
    { }

    public UserViewModel(User user)
    {
        Id = user.Id;
        Active = user.Active;
        Name = user.Name;
        Email = user.Email;
        EmailConfirmed = user.EmailConfirmed;
        Language = user.Language;
        Theme = user.Theme;
    }

    public UserId Id { get; set; }
    public bool Active { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public Language Language { get; set; }
    public Theme Theme { get; set; }
}