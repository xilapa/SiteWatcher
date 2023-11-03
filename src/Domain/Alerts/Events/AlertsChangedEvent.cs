using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.Events;

public sealed class AlertsChangedEvent
{
    public AlertsChangedEvent(UserId userId)
    {
        UserId = userId;
    }

    public UserId UserId { get; set; }
}