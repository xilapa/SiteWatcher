using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Worker.Messaging;

public class WatchAlertsMessage
{
    public WatchAlertsMessage(IEnumerable<EFrequency> frequencies)
    {
        Frequencies = frequencies;
    }

    public IEnumerable<EFrequency> Frequencies { get; }
}