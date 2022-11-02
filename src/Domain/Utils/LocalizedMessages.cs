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

    public static string AlertNotificationMessageSubject(ELanguage language)
    {
        return language switch
        {
            ELanguage.English => "Your alerts have been triggered",
            ELanguage.Spanish => "Tus alertas han sido activadas",
            ELanguage.BrazilianPortuguese => "Seus alertas foram disparados"
        };
    }

    public static string AlertNotificationMessageTemplate(ELanguage language)
    {
        return language switch
        {
            ELanguage.English => @"
                    Hello {{UserName}}!
                    <br>
                    {% if HasSuccess %}
                      <br>
                      The following alerts have been triggered:
                      <ul>
                          {% for alert in AlertsToNotifySuccess %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} : {{alert.WatchModeString}}
                          </li>
                          {% endfor %}
                      </ul>
                    {% endif %}
                        
                    {% if HasErrors %}
                      <br>
                      The following alerts sites couldn't be reached:
                      <ul>
                          {% for alert in AlertsToNotifyError %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} : {{alert.WatchModeString}}
                          </li>
                          {% endfor %}
                      </ul>
					Please verify the site and edit the alert. If the site is working, this could be caused by a transient failure and can be ignored.<br>
                    Anyway, the site will be verified on the next scheduled frequency.
                    {% endif %}

                    <br>
                    Email sent automatically by <a href=""{{SiteWatcherUri}}"">SiteWatcher</a>",

            ELanguage.Spanish => @"
                    Hola {{UserName}}!
                    <br>
                    {% if HasSuccess %}
                      <br>
                      Se han activado las siguientes alertas:
                      <ul>
                          {% for alert in AlertsToNotifySuccess %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} : {{alert.WatchModeString}}
                          </li>
                          {% endfor %}
                      </ul>
                    {% endif %}
                        
                    {% if HasErrors %}
                      <br>
                      No se pudo acceder a los siguientes sitios de alertas:
                      <ul>
                          {% for alert in AlertsToNotifyError %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} : {{alert.WatchModeString}}
                          </li>
                          {% endfor %}
                      </ul>					
					Verifique el sitio y edite la alerta. Si el sitio está funcionando, esto podría deberse a una falla transitoria y puede ignorarse.<br>
                    De todos modos, el sitio será verificado en la próxima frecuencia programada.
                    {% endif %}

                    <br>
                    Correo electrónico enviado automáticamente por <a href=""{{SiteWatcherUri}}"">SiteWatcher</a>",

            ELanguage.BrazilianPortuguese => @"
                    Olá {{UserName}}!
                    <br>
                    {% if HasSuccess %}
                      <br>
                      Os seguintes alertas foram disparados:
                      <ul>
                          {% for alert in AlertsToNotifySuccess %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} : {{alert.WatchModeString}}
                          </li>
                          {% endfor %}
                      </ul>
                    {% endif %}
                        
                    {% if HasErrors %}
                      <br>
                      No se pudo acceder a los siguientes sitios de alertas:
                      <ul>
                          {% for alert in AlertsToNotifyError %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} : {{alert.WatchModeString}}
                          </li>
                          {% endfor %}
                      </ul>
					Verifique o site e edite o alerta. Se o site estiver funcionando, isso pode ser causado por uma falha temporária e pode ser ignorado.<br>
                    De qualquer forma, o site será verificado na próxima frequência programada.
                    {% endif %}

                    <br>
                    Email enviado automaticamente por <a href=""{{SiteWatcherUri}}"">SiteWatcher</a>"
        };
    }

    public static string WatchModeString(ELanguage language, EWatchMode watchMode)
    {
        return language switch
        {
            ELanguage.English => watchMode switch
            {
                EWatchMode.AnyChanges => "Monitoring type: Any Changes",
                EWatchMode.Term => "Monitoring type: Term"
            },
            ELanguage.Spanish => watchMode switch
            {
                EWatchMode.AnyChanges => "Tipo de monitoreo: Algún cambio",
                EWatchMode.Term => "Tipo de monitoreo: Algún cambio"
            },
            ELanguage.BrazilianPortuguese => watchMode switch
            {
                EWatchMode.AnyChanges => "Tipo de monitoramento: Quaisquer mudanças",
                EWatchMode.Term => "Tipo de monitoramento: Termo"
            }
        };
    }

    public static string FrequencyString(ELanguage language, EFrequency frequency)
    {
        return language switch
        {
            ELanguage.English => frequency switch
            {
                EFrequency.TwoHours => "Every two hours",
                EFrequency.FourHours => "Every four hours",
                EFrequency.EightHours => "Every eight hours",
                EFrequency.TwelveHours => "Every twelve hours",
                EFrequency.TwentyFourHours => "Every twenty four hours"
            },
            ELanguage.Spanish => frequency switch
            {
                EFrequency.TwoHours => "Cada dos horas",
                EFrequency.FourHours => "Cada cuatro horas",
                EFrequency.EightHours => "Cada ocho horas",
                EFrequency.TwelveHours => "Cada doce horas",
                EFrequency.TwentyFourHours => "Cada veinticuatro horas"
            },
            ELanguage.BrazilianPortuguese => frequency switch
            {
                EFrequency.TwoHours => "A cada duas horas",
                EFrequency.FourHours => "A cada quatro horas",
                EFrequency.EightHours => "A cada oito horas",
                EFrequency.TwelveHours => "A cada doze horas",
                EFrequency.TwentyFourHours => "A cada vinte e quatro horas"
            }
        };
    }
}