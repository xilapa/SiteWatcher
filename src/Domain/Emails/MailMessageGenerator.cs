using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Emails;

// TODO: Should be Email Factory, and MailMessage Should Be On EmailCreatedEvent
public static class MailMessageGenerator
{
    public static MailMessage EmailConfirmation(string userName, string email, string confirmationLink, Language language) =>
        new()
        {
            Recipients = new[] { new MailRecipient { Email = email, Name = userName } },
            Subject = LocalizedMessages.EmailConfirmationSubject(language),
            HtmlBody = true,
            Body = LocalizedMessages.EmailConfirmationBody(language, confirmationLink)
        };

    public static MailMessage AccountActivation(string userName, string email, string confirmationLink, Language language) =>
        new()
        {
            Recipients = new[] { new MailRecipient { Email = email, Name = userName } },
            Subject = LocalizedMessages.AccountActivationSubject(language),
            HtmlBody = true,
            Body = LocalizedMessages.AccountActivationBody(language, confirmationLink)
        };

    public static MailMessage AccountDeleted(string userName, string email, Language language) =>
        new()
        {
            Recipients = new[] { new MailRecipient { Email = email, Name = userName } },
            Subject = LocalizedMessages.AccountDeletedSubject(language),
            HtmlBody = true,
            Body = LocalizedMessages.AccountDeletedBody(language)
        };
}