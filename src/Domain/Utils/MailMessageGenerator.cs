using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Email;

namespace SiteWatcher.Domain.Utils;

public static class MailMessageGenerator
{
    public static MailMessage EmailConfirmation(string userName, string email, string confirmationLink, ELanguage language)
    {
        var message = new MailMessage
        {
            Recipients = new []{ new MailRecipient{Email = email, Name = userName} },
            Subject = LocalizedMessages.EmailConfirmationSubject(language),
            HtmlBody = true,
            Body = LocalizedMessages.EmailConfirmationBody(language, confirmationLink)
        };
        return message;
    }

    public static MailMessage AccountActivation(string userName, string email, string confirmationLink, ELanguage language)
    {
        var message = new MailMessage
        {
            Recipients = new []{ new MailRecipient{Email = email, Name = userName} },
            Subject = LocalizedMessages.AccountActivationSubject(language),
            HtmlBody = true,
            Body = LocalizedMessages.AccountActivationBody(language, confirmationLink)
        };
        return message;
    }

    public static MailMessage AccountDeleted(string userName, string email, ELanguage language)
    {
        var message = new MailMessage
        {
            Recipients = new []{ new MailRecipient{Email = email, Name = userName} },
            Subject = LocalizedMessages.AccountDeletedSubject(language),
            HtmlBody = true,
            Body = LocalizedMessages.AccountDeletedBody(language)
        };
        return message;
    }
}