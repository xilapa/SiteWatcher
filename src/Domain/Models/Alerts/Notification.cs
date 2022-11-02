using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Domain.Models.Emails;

namespace SiteWatcher.Domain.Models.Alerts;

public class Notification
{
    // ctor for ef
    protected Notification()
    { }

    public Notification(DateTime currentTime)
    {
        Id = NotificationId.New();
        CreatedAt = currentTime;
    }

    public NotificationId Id { get; set; }
    public DateTime CreatedAt { get; }
    public Email? Email { get; set; }
}