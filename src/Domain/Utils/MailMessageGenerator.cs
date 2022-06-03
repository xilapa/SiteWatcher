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
}

public static class LocalizedMessages
{
    public static string EmailConfirmationSubject(ELanguage language)
    {
        return language switch
        {
            ELanguage.English => "Email Confirmation",
            ELanguage.Spanish => "Confirmación de correo electrónico",
            ELanguage.BrazilianPortuguese => "Confirmação de e-mail"
        };
    }

    public static string EmailConfirmationBody(ELanguage language, string link)
    {
        return language switch
        {
            ELanguage.English => $"Please confirm your email address by clicking on the link below: <a href='{link}'>Confirm Email</a>",
            ELanguage.Spanish => $"Confirme su dirección de correo electrónico haciendo clic en el siguiente enlace: <a href='{link}'>Confirmar correo electrónico</a>",
            ELanguage.BrazilianPortuguese => $"Confirme seu endereço de e-mail clicando no link abaixo: <a href='{link}'>Confirmar e-mail</a>"
        };
    }
}