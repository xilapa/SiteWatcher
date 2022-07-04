using SiteWatcher.Domain.Models.Common;

namespace Domain.Events.Alerts;

public class AlertCreatedOrUpdatedEvent : BaseEvent
{
    public AlertCreatedOrUpdatedEvent(UserId userId)
    {
        UserId = userId;
    }

    public UserId UserId { get; set; }
}