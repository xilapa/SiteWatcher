using SiteWatcher.Domain.Enums;

namespace Domain.Events;

public class AccountDeletedEvent : BaseEvent
{
    public AccountDeletedEvent(string name, string email, ELanguage language)
    {
        Name = name;
        Email = email;
        Language = language;
    }

    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
}