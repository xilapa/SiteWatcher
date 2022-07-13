using SiteWatcher.Domain.Models.Common;

namespace Domain.Events.Alerts;

public class AlertsChangedEvent : BaseEvent
{
    public AlertsChangedEvent(UserId userId)
    {
        UserId = userId;
    }

    public UserId UserId { get; set; }
}