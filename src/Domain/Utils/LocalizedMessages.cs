using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.Utils;

public static class LocalizedMessages
{
    public static string EmailConfirmationSubject(ELanguage language)
    {
        return language switch
        {
            ELanguage.English => "Email confirmation",
            ELanguage.Spanish => "Confirmación de correo electrónico",
            ELanguage.BrazilianPortuguese => "Confirmação de e-mail"
        };
    }

    public static string EmailConfirmationBody(ELanguage language, string link)
    {
        return language switch
        {
            ELanguage.English => $"Please confirm your email address by clicking on the link below: <a href='{link}'>Confirm email</a>",
            ELanguage.Spanish => $"Confirme su dirección de correo electrónico haciendo clic en el siguiente enlace: <a href='{link}'>Confirmar correo electrónico</a>",
            ELanguage.BrazilianPortuguese => $"Confirme seu endereço de e-mail clicando no link abaixo: <a href='{link}'>Confirmar e-mail</a>"
        };
    }

    public static string AccountActivationSubject(ELanguage language)
    {
        return language switch
        {
            ELanguage.English => "Account activation",
            ELanguage.Spanish => "Activación de cuenta",
            ELanguage.BrazilianPortuguese => "Ativação de conta"
        };
    }

    public static string AccountActivationBody(ELanguage language, string link)
    {
        return language switch
        {
            ELanguage.English => $"Please activate your account by clicking on the link below: <a href='{link}'>Activate account</a>",
            ELanguage.Spanish => $"Activa tu cuenta haciendo clic en el siguiente enlace: <a href='{link}'>Activar cuenta</a>",
            ELanguage.BrazilianPortuguese => $"Ative sua conta clicando no link abaixo: <a href='{link}'>Ativar conta</a>"
        };
    }

    public static string AccountDeletedSubject(ELanguage language)
    {
        return language switch
        {
            ELanguage.English => "Account deleted",
            ELanguage.Spanish => "Cuenta eliminada",
            ELanguage.BrazilianPortuguese => "Conta deletada"
        };
    }
    public static string AccountDeletedBody(ELanguage language)
    {
        return language switch
        {
            ELanguage.English => "Your account and all of your data on SiteWatcher has been deleted.",
            ELanguage.Spanish => "Su cuenta y todos sus datos en SiteWatcher han sido eliminados.",
            ELanguage.BrazilianPortuguese => "Sua conta e todos os seus dados no SiteWatcher foram excluídos."
        };
    }
}