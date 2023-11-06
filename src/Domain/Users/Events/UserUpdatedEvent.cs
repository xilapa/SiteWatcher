using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Users.Events;

public class UserUpdatedEvent : BaseEvent
{
    public UserUpdatedEvent(UserId userId)
    {
        UserId = userId;
    }

    public UserId UserId { get; set; }
}