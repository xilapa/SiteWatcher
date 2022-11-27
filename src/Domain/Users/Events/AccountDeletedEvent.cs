using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Users.Events;

public class AccountDeletedEvent : BaseEvent
{
    public AccountDeletedEvent(string name, string email, Language language)
    {
        Name = name;
        Email = email;
        Language = language;
    }

    public string Name { get; set; }
    public string Email { get; set; }
    public Language Language { get; set; }
}