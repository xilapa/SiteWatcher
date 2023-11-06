using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.Events;

public sealed class AlertsChangedEvent : BaseEvent
{
    public AlertsChangedEvent(UserId userId)
    {
        UserId = userId;
    }

    public UserId UserId { get; set; }
}