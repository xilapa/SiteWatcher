using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Domain.Notifications;

public class Notification
{
    // for EF
    protected Notification(){}

    public Notification(AlertsTriggeredEvent @event, DateTime currentDate, string siteWatcherUri)
    {
        Id = NotificationId.New();
        CreatedAt = currentDate;
        UserId = @event.UserId;
        _notificationAlerts = new List<NotificationAlert>();
        _notificationData = new NotificationData(@event.UserName, @event.UserLanguage, @event.UserEmail, siteWatcherUri);

        foreach (var alertTriggered in @event.Alerts)
        {
            // Create the relation between the notification and the alert
            _notificationAlerts.Add(new NotificationAlert(Id, alertTriggered.AlertId, alertTriggered.TriggeringDate));

            // Save the alert triggering data
            AddAlertTriggered(alertTriggered);
        }
    }

    public NotificationId Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public UserId UserId { get; private set; }
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
        // Create the email, the email will generate a domain event
        var recipient = new MailRecipient(_notificationData.UserName, _notificationData.Email, UserId);
        Email = new Email(body,htmlBody: true, subject, recipient, currentDate);
        EmailId = Email.Id;
    }
}

public class NotificationAlert
{
    public NotificationAlert(NotificationId notificationId, AlertId alertId, DateTime triggeringDate)
    {
        NotificationId = notificationId;
        AlertId = alertId;
        TriggeringDate = triggeringDate;
    }

    public NotificationId NotificationId { get; private set; }
    public AlertId AlertId { get; private set; }
    public DateTime TriggeringDate { get; private set; }
}