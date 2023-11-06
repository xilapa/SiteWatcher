using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Entities.Triggerings;
using SiteWatcher.Domain.Alerts.Messages;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Domain.Notifications;

public class Notification
{
    // for EF
    protected Notification(){}

    public Notification(AlertsTriggeredMessage message, DateTime currentDate, string siteWatcherUri)
    {
        Id = NotificationId.New();
        CreatedAt = currentDate;
        UserId = message.UserId;
        _notificationAlerts = new List<NotificationAlert>();
        _notificationData = new NotificationData(message.UserName, message.UserLanguage, message.UserEmail, siteWatcherUri);

        foreach (var alertTriggered in message.Alerts)
        {
            // Create the relation between the notification and the alert
            _notificationAlerts.Add(new NotificationAlert(Id, alertTriggered));

            // Save the alert triggering data
            AddAlertTriggered(alertTriggered);
        }
    }

    public NotificationId Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public UserId? UserId { get; private set; }
    public User User { get; private set; } = null!;
    public EmailId? EmailId { get;  private set; }
    public Email Email { get; private set; } = null!;
    public IReadOnlyCollection<Alert> Alerts { get; private set; }

    private readonly List<NotificationAlert> _notificationAlerts;
    public IReadOnlyCollection<NotificationAlert> NotificationAlerts => _notificationAlerts.AsReadOnly();

    private readonly NotificationData _notificationData;

    public void AddAlertTriggered(AlertTriggered alertTriggered)
    {
        _notificationData.AddAlertTriggered(alertTriggered);
    }

    /// <summary>
    /// Process notification messages and dispatch them with Domain Events.
    /// </summary>
    public async Task ProcessAndDispatch(DateTime currentDate)
    {
        // Get Body and Subject
        var subject = NotificationMessageGenerator.GetSubject(_notificationData);
        var body = await NotificationMessageGenerator.GetBody(_notificationData);
        // Create the email, the email will generate a domain message
        var recipient = new MailRecipient(_notificationData.UserName, _notificationData.Email, UserId!.Value);
        Email = new Email(body,htmlBody: true, subject, recipient, currentDate);
        EmailId = Email.Id;
    }
}

public class NotificationAlert
{
    protected NotificationAlert(){}

    public NotificationAlert(NotificationId notificationId, AlertTriggered alertTriggered)
    {
        NotificationId = notificationId;
        AlertId = alertTriggered.AlertId;
        TriggeringDate = alertTriggered.TriggeringDate;
        TriggeringStatus = alertTriggered.Status;
    }

    public NotificationId NotificationId { get; private set; }
    public AlertId AlertId { get; private set; }
    public DateTime TriggeringDate { get; private set; }
    public TriggeringStatus TriggeringStatus { get; private set; }
}