using SiteWatcher.Domain.Enums;

namespace Worker.Messaging;

public class WatchAlertsMessage
{
    public WatchAlertsMessage(EFrequency frequency)
    {
        Frequency = frequency;
    }

    public EFrequency Frequency { get; }
}