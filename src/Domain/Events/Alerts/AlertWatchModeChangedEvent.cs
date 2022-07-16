using SiteWatcher.Domain.Models.Common;

namespace Domain.Events.Alerts;

public class AlertWatchModeChangedEvent : BaseEvent
{
    public AlertWatchModeChangedEvent(WatchModeId oldWatchModeId)
    {
        OldWatchModeId = oldWatchModeId;
    }

    public WatchModeId OldWatchModeId { get; }
}