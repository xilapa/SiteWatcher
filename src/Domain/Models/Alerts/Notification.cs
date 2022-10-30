using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.Models.Alerts;

public class Notification
{
    // ctor for ef
    protected Notification()
    { }

    public Notification(DateTime currentTime)
    {
        Id = new NotificationId();
        CreatedAt = currentTime;
    }

    public NotificationId Id { get; set; }
    public DateTime CreatedAt { get; private set; }
}