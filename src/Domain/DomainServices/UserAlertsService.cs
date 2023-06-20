using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Domain.DomainServices;

public sealed class UserAlertsService : IUserAlertsService
{
    private readonly IHttpClient _httpClient;

    public UserAlertsService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ExecuteAlerts(User user, DateTime currentTime, CancellationToken ct)
    {
        var alertsTriggered = new List<AlertTriggered>();
        foreach (var alert in user.Alerts)
        {
            var htmlStream = await _httpClient.GetStreamAsync(alert.Site.Uri, ct);
            var alertTriggered = await alert.ExecuteRule(htmlStream, currentTime);
            if (alertTriggered != null)
                alertsTriggered.Add(alertTriggered);
        }

        if (alertsTriggered.Count == 0) return;
        user.AddDomainEvent(new AlertsTriggeredEvent(user, alertsTriggered, currentTime));
    }

    // TODO: Generate mail message will be done on the notification processor
    // private async Task<MailMessage> GenerateMailMessage(User user, string siteWatcherUri, List<AlertToNotify> alertsToNotify)
    // {
    //     var subject = NotificationMessageGenerator.GetSubject(user.Language);
    //     var body = await NotificationMessageGenerator.GetBody(user, alertsToNotify, siteWatcherUri);
    //     var emailRecipient = new MailRecipient(user.Name, user.Email, user.Id);
    //
    //     // generate the email and the email msg
    //     var email = new Email(body, subject, emailRecipient);
    //     var emailMsg = new MailMessage(email.Id, subject, body, true, emailRecipient);
    //
    //     // Save the email related to the MailMessage that will be sent
    //     // The DateSent will be set by who sent the email
    //     _emailRepository.Add(email);
    //
    //     // correlate the notifications with the email entity
    //     var notificationIds = alertsToNotify.Select(_ => _.NotificationId).ToArray();
    //     foreach(var alert in user.Alerts)
    //         alert.SetEmail(email, notificationIds);
    //
    //     return emailMsg;
    // }
}

public interface IUserAlertsService
{
    /// <summary>
    /// Executes all user alerts. If there are no notification for the user, null is returned.
    /// </summary>
    /// <param name="user">User with the alerts and rules loaded</param>
    /// <param name="currentTime">Current time</param>
    /// <param name="ct">Cancellation token</param>
    Task ExecuteAlerts(User user, DateTime currentTime, CancellationToken ct);
}