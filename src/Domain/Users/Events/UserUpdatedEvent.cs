using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Users.Events;

public class UserUpdatedEvent
{
    public UserUpdatedEvent(UserId userId)
    {
        UserId = userId;
    }

    public UserId UserId { get; set; }
}