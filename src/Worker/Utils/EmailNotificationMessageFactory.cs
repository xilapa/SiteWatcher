using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Emails;
using SiteWatcher.Domain.Utils.Notifications;
using SiteWatcher.Worker.Messaging;

namespace SiteWatcher.Worker.Utils;

public static class EmailNotificationMessageFactory
{
    public static async Task<EmailNotificationMessage> Generate(User user, List<AlertToNotify> successes, List<AlertToNotify> errors, string siteWatcherUri)
    {
        var subject = NotificationMessageGenerator.GetSubject(user.Language);
        var body = await NotificationMessageGenerator.GetBody(user, successes, errors, siteWatcherUri);
        var emailRecipient = new MailRecipient(user.Name, user.Email);
        return new EmailNotificationMessage(subject, body, emailRecipient);
    }
}