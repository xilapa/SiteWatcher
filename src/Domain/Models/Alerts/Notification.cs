using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.Models.Alerts;

public class Notification
{
    // ctor for ef
    protected Notification()
    { }

    public Notification(DateTime currentTime)
    {
        CreationDate = currentTime;
    }

    public DateTime CreationDate { get; private set; }
}