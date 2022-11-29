using SiteWatcher.Domain.Alerts.Entities.Notifications;
using SiteWatcher.Domain.Alerts.ValueObjects;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users;
using SiteWatcher.Worker.Messaging;

namespace SiteWatcher.Worker.Utils;

public static class EmailNotificationMessageFactory
{
    public static async Task<(Email, EmailNotificationMessage)> Generate(User user, List<AlertToNotify> successes, List<AlertToNotify> errors, string siteWatcherUri)
    {
        var subject = NotificationMessageGenerator.GetSubject(user.Language);
        var body = await NotificationMessageGenerator.GetBody(user, successes, errors, siteWatcherUri);
        var emailRecipient = new MailRecipient(user.Name, user.Email, user.Id);
        var notificationMsg = new EmailNotificationMessage(subject, body, emailRecipient);

        var email = new Email(body, subject, emailRecipient);
        return (email, notificationMsg);
    }
}