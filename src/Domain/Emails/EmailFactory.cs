using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Emails.Messages;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Messages;

namespace SiteWatcher.Domain.Emails;

public static class EmailFactory
{
    public static (Email, EmailCreatedMessage) EmailConfirmation(EmailConfirmationTokenGeneratedMessage msg,
        string confirmationLink, DateTime currentDate)
    {
        var body = LocalizedMessages.EmailConfirmationBody(msg.Language, confirmationLink);
        var subject = LocalizedMessages.EmailConfirmationSubject(msg.Language);
        var mailRecipient = new MailRecipient(msg.Name, msg.Email, msg.UserId);
        return Email.CreateEmail(body, htmlBody: true, subject, mailRecipient, currentDate);
    }

    public static (Email, EmailCreatedMessage) AccountActivation(UserReactivationTokenGeneratedMessage msg, string confirmationLink,
        DateTime currentDate)
    {
        var body = LocalizedMessages.AccountActivationBody(msg.Language, confirmationLink);
        var subject = LocalizedMessages.AccountActivationSubject(msg.Language);
        var mailRecipient = new MailRecipient(msg.Name, msg.Email, msg.UserId);
        return Email.CreateEmail(body, htmlBody: true, subject, mailRecipient, currentDate);
    }

    public static (Email, EmailCreatedMessage) AccountDeleted(User user, DateTime currentDate)
    {
        var body = LocalizedMessages.AccountDeletedBody(user.Language);
        var subject = LocalizedMessages.AccountDeletedSubject(user.Language);
        return Email.CreateEmail(body, htmlBody: true, subject, user, currentDate);
    }
}