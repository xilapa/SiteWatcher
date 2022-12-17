using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Entities.Notifications;
using SiteWatcher.Domain.Alerts.ValueObjects;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Emails.Repositories;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Domain.DomainServices;

public sealed class UserAlertsService : IUserAlertsService
{
    private readonly IHttpClient _httpClient;
    private readonly IEmailRepository _emailRepository;

    public UserAlertsService(IHttpClient httpClient, IEmailRepository emailRepository)
    {
        _httpClient = httpClient;
        _emailRepository = emailRepository;
    }

    public async Task<NotificationToSend?> ExecuteAlerts(User user, DateTime currentTime, string siteWatcherUri, CancellationToken ct)
    {
        var alertsToNotify = new List<AlertToNotify>();
        foreach (var alert in user.Alerts)
        {
            var htmlStream = await _httpClient.GetStreamAsync(alert.Site.Uri, ct);
            var alertToNotify = await alert.ExecuteRule(htmlStream, currentTime);
            if (alertToNotify != null)
                alertsToNotify.Add(alertToNotify);
        }
        if(alertsToNotify.Count == 0)
            return null;

        var mailMsg = await GenerateMailMessage(user, siteWatcherUri, alertsToNotify);
        return new NotificationToSend
        {
            EmailNotification = mailMsg
        };
    }

    private async Task<MailMessage> GenerateMailMessage(User user, string siteWatcherUri, List<AlertToNotify> alertsToNotify)
    {
        var subject = NotificationMessageGenerator.GetSubject(user.Language);
        var body = await NotificationMessageGenerator.GetBody(user, alertsToNotify, siteWatcherUri);
        var emailRecipient = new MailRecipient(user.Name, user.Email, user.Id);

        // generate the email and the email msg
        var email = new Email(body, subject, emailRecipient);
        var emailMsg = new MailMessage(email.Id, subject, body, true, emailRecipient);

        // Save the email related to the MailMessage that will be sent
        // The DateSent will be set by who sent the email
        _emailRepository.Add(email);

        // correlate the notifications with the email entity
        foreach(var alert in user.Alerts)
            alert.SetEmail(email, alertsToNotify.Select(_ => _.NotificationId));

        return emailMsg;
    }
}

public sealed class NotificationToSend
{
    public MailMessage? EmailNotification { get; set; }
}

public interface IUserAlertsService
{
    /// <summary>
    /// Executes all user alerts. If there are no notification for the user, null is returned.
    /// </summary>
    /// <param name="user">User with the alerts and rules loaded</param>
    /// <param name="currentTime">Current time</param>
    /// <returns>The notification to send</returns>
    Task<NotificationToSend?> ExecuteAlerts(User user, DateTime currentTime, string siteWatcherUri, CancellationToken ct);
}