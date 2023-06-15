using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Domain.Notifications;

public class Notification : BaseModel<NotificationId>
{
    // ctor for EF
    protected Notification()
    { }

    public Notification(UserId userId, DateTime currentDate) : base(NotificationId.New(), currentDate)
    {
        UserId = userId;
        Id = NotificationId.New();
        _notificationAlerts = new List<NotificationAlert>();
    }

    public UserId UserId { get; private set; }
    public User User { get; private set; }
    public EmailId? EmailId { get;  private set; }
    public Email Email { get;  private set; }
    public List<Alert> Alerts { get; private set; }

    private readonly List<NotificationAlert> _notificationAlerts;

    public IReadOnlyCollection<NotificationAlert> NotificationAlerts => _notificationAlerts.AsReadOnly();
}

public class NotificationAlert
{
    public NotificationId NotificationId { get; set; }
    public AlertId AlertId { get; set; }
}