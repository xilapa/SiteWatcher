using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;

namespace SiteWatcher.Domain.Alerts.Entities.Notifications;

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

    public void SetEmail(Email email) => Email = email;
}