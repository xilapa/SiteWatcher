using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Domain.DomainServices;

public static class AlertFrequencies
{
    public static List<Frequencies> GetCurrentFrequencies(DateTime currentTime)
    {
        var alertFrequenciesToWatch = new List<Frequencies>();

        var currentHour = currentTime.Hour;

        // If the rest of current hour/ frequency is zero, this alerts of this frequency needs to be watched
        foreach (var frequency in Enum.GetValues<Frequencies>())
        {
            if (currentHour % (int)frequency == 0)
                alertFrequenciesToWatch.Add(frequency);
        }

        return alertFrequenciesToWatch;
    }
}