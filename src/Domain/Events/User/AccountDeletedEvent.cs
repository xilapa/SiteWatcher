using SiteWatcher.Domain.Enums;

namespace Domain.Events;

public class AccountDeletedEvent : BaseEvent
{
    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
}