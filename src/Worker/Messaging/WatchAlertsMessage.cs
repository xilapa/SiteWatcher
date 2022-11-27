using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Worker.Messaging;

public class WatchAlertsMessage
{
    public WatchAlertsMessage(IEnumerable<Frequencies> frequencies)
    {
        Frequencies = frequencies;
    }

    public IEnumerable<Frequencies> Frequencies { get; }
}