using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.Events;

public sealed class AlertWatchModeChangedEvent : BaseEvent
{
    public AlertWatchModeChangedEvent(WatchModeId oldWatchModeId)
    {
        OldWatchModeId = oldWatchModeId;
    }

    public WatchModeId OldWatchModeId { get; }
}