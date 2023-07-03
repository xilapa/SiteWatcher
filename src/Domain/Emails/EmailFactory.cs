using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Domain.Emails;

public static class EmailFactory
{
    public static Email AccountConfirmation(User user, string confirmationLink, DateTime currentDate)
    {
        var body = LocalizedMessages.EmailConfirmationBody(user.Language, confirmationLink);
        var subject = LocalizedMessages.EmailConfirmationSubject(user.Language);
        return new Email(body, htmlBody: true, subject, user, currentDate);
    }

    public static Email AccountActivation(User user, string confirmationLink, DateTime currentDate)
    {
        var body = LocalizedMessages.AccountActivationBody(user.Language, confirmationLink);
        var subject = LocalizedMessages.AccountActivationSubject(user.Language);
        return new Email(body, htmlBody: true, subject, user, currentDate);
    }

    public static Email AccountDeleted(User user, DateTime currentDate)
    {
        var body = LocalizedMessages.AccountDeletedBody(user.Language);
        var subject = LocalizedMessages.AccountDeletedSubject(user.Language);
        return new Email(body, htmlBody: true, subject, user, currentDate);
    }
}