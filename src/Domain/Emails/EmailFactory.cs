using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Messages;

namespace SiteWatcher.Domain.Emails;

public static class EmailFactory
{
    public static Email EmailConfirmation(EmailConfirmationTokenGeneratedMessage msg, string confirmationLink, DateTime currentDate)
    {
        var body = LocalizedMessages.EmailConfirmationBody(msg.Language, confirmationLink);
        var subject = LocalizedMessages.EmailConfirmationSubject(msg.Language);
        var mailRecipient = new MailRecipient(msg.Name, msg.Email, msg.UserId);
        return new Email(body, htmlBody: true, subject, mailRecipient, currentDate);
    }

    public static Email AccountActivation(UserReactivationTokenGeneratedMessage msg, string confirmationLink, DateTime currentDate)
    {
        var body = LocalizedMessages.AccountActivationBody(msg.Language, confirmationLink);
        var subject = LocalizedMessages.AccountActivationSubject(msg.Language);
        var mailRecipient = new MailRecipient(msg.Name, msg.Email, msg.UserId);
        return new Email(body, htmlBody: true, subject, mailRecipient, currentDate);
    }

    public static Email AccountDeleted(User user, DateTime currentDate)
    {
        var body = LocalizedMessages.AccountDeletedBody(user.Language);
        var subject = LocalizedMessages.AccountDeletedSubject(user.Language);
        return new Email(body, htmlBody: true, subject, user, currentDate);
    }
}