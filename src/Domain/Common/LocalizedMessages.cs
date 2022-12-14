using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Common;

public static class LocalizedMessages
{
    public static string EmailConfirmationSubject(Language language)
    {
        return language switch
        {
            Language.English => "Email confirmation",
            Language.Spanish => "Confirmación de correo electrónico",
            Language.BrazilianPortuguese => "Confirmação de e-mail",
            _ => throw new NotImplementedException()
        };
    }

    public static string EmailConfirmationBody(Language language, string link)
    {
        return language switch
        {
            Language.English => $"Please confirm your email address by clicking on the link below: <a href='{link}'>Confirm email</a>",
            Language.Spanish => $"Confirme su dirección de correo electrónico haciendo clic en el siguiente enlace: <a href='{link}'>Confirmar correo electrónico</a>",
            Language.BrazilianPortuguese => $"Confirme seu endereço de e-mail clicando no link abaixo: <a href='{link}'>Confirmar e-mail</a>",
            _ => throw new NotImplementedException()
        };
    }

    public static string AccountActivationSubject(Language language)
    {
        return language switch
        {
            Language.English => "Account activation",
            Language.Spanish => "Activación de cuenta",
            Language.BrazilianPortuguese => "Ativação de conta",
            _ => throw new NotImplementedException()
        };
    }

    public static string AccountActivationBody(Language language, string link)
    {
        return language switch
        {
            Language.English => $"Please activate your account by clicking on the link below: <a href='{link}'>Activate account</a>",
            Language.Spanish => $"Activa tu cuenta haciendo clic en el siguiente enlace: <a href='{link}'>Activar cuenta</a>",
            Language.BrazilianPortuguese => $"Ative sua conta clicando no link abaixo: <a href='{link}'>Ativar conta</a>",
            _ => throw new NotImplementedException()
        };
    }

    public static string AccountDeletedSubject(Language language)
    {
        return language switch
        {
            Language.English => "Account deleted",
            Language.Spanish => "Cuenta eliminada",
            Language.BrazilianPortuguese => "Conta deletada",
            _ => throw new NotImplementedException()
        };
    }
    public static string AccountDeletedBody(Language language)
    {
        return language switch
        {
            Language.English => "Your account and all of your data on SiteWatcher has been deleted.",
            Language.Spanish => "Su cuenta y todos sus datos en SiteWatcher han sido eliminados.",
            Language.BrazilianPortuguese => "Sua conta e todos os seus dados no SiteWatcher foram excluídos.",
            _ => throw new NotImplementedException()
        };
    }

    public static string AlertNotificationMessageSubject(Language language)
    {
        return language switch
        {
            Language.English => "Your alerts have been triggered",
            Language.Spanish => "Tus alertas han sido activadas",
            Language.BrazilianPortuguese => "Seus alertas foram disparados",
            _ => throw new NotImplementedException()
        };
    }

    // TODO: move these htmls to a separated file
    public static string AlertNotificationMessageTemplate(Language language)
    {
        return language switch
        {
            Language.English =>
                    @"Hello {{UserName}}!
                    <br>
                    {% if HasSuccess %}
                      <br>
                      The following alerts have been triggered:
                      <ul>
                          {% for alert in AlertsToNotifySuccess %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} : {{alert.RuleString}}
                          </li>
                          <br>
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
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} - {{alert.RuleString}}
                          </li>
                          <br>
                          {% endfor %}
                      </ul>
					Please verify the site and edit the alert. If the site is working, this could be caused by a transient failure and can be ignored.<br>
                    Anyway, the site will be verified on the next scheduled frequency.<br><br>
                    {% endif %}

                    Email sent automatically by <a href=""{{SiteWatcherUri}}"">SiteWatcher</a>",

            Language.Spanish =>
                    @"Hola {{UserName}}!
                    <br>
                    {% if HasSuccess %}
                      <br>
                      Se han activado las siguientes alertas:
                      <ul>
                          {% for alert in AlertsToNotifySuccess %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} - {{alert.RuleString}}
                          </li>
                          <br>
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
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} - {{alert.RuleString}}
                          </li>
                          <br>
                          {% endfor %}
                      </ul>					
					Verifique el sitio y edite la alerta. Si el sitio está funcionando, esto podría deberse a una falla transitoria y puede ignorarse.<br>
                    De todos modos, el sitio será verificado en la próxima frecuencia programada.<br><br>
                    {% endif %}

                    Correo electrónico enviado automáticamente por <a href=""{{SiteWatcherUri}}"">SiteWatcher</a>",

            Language.BrazilianPortuguese =>
                    @"Olá {{UserName}}!
                    <br>
                    {% if HasSuccess %}
                      <br>
                      Os seguintes alertas foram disparados:
                      <ul>
                          {% for alert in AlertsToNotifySuccess %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} - {{alert.RuleString}}
                          </li>
                          <br>
                          {% endfor %}
                      </ul>
                    {% endif %}
                        
                    {% if HasErrors %}
                      <br>
                      Não foi possível acessar os seguintes sites de alertas:
                      <ul>
                          {% for alert in AlertsToNotifyError %}
                          <li>
                              <b>{{alert.Name}}</b>
                              <br>
                              <a href=""{{alert.SiteUri}}"">{{alert.SiteName}}</a> - {{alert.FrequencyString}} - {{alert.RuleString}}
                          </li>
                          <br>
                          {% endfor %}
                      </ul>
					Verifique o site e edite o alerta. Se o site estiver funcionando, isso pode ser causado por uma falha temporária e pode ser ignorado.<br>
                    De qualquer forma, o site será verificado na próxima frequência programada.<br><br>
                    {% endif %}

                    Email enviado automaticamente por <a href=""{{SiteWatcherUri}}"">SiteWatcher</a>",
            _ => throw new NotImplementedException()
        };
    }

    public static string RuleString(Language language, Rules rule)
    {
        return language switch
        {
            Language.English => rule switch
            {
                Rules.AnyChanges => "Monitoring rule: Any Changes",
                Rules.Term => "Monitoring rule: Term",
                Rules.Regex => "Monitoring rule: Regex Pattern",
                _ => throw new NotImplementedException()
            },
            Language.Spanish => rule switch
            {
                Rules.AnyChanges => "Regla de monitoreo: Algún cambio",
                Rules.Term => "Regla de monitoreo: Algún cambio",
                Rules.Regex => "Regla de monitoreo: Patrón de regex",
                _ => throw new NotImplementedException()
            },
            Language.BrazilianPortuguese => rule switch
            {
                Rules.AnyChanges => "Regra de monitoramento: Quaisquer mudanças",
                Rules.Term => "Regra de monitoramento: Termo",
                Rules.Regex => "Regra de monitoramento: Padrão de regex",
                _ => throw new NotImplementedException()
            },
            _ => throw new NotImplementedException()
        };
    }

    public static string FrequencyString(Language language, Frequencies frequency)
    {
        return language switch
        {
            Language.English => frequency switch
            {
                Frequencies.TwoHours => "Every two hours",
                Frequencies.FourHours => "Every four hours",
                Frequencies.EightHours => "Every eight hours",
                Frequencies.TwelveHours => "Every twelve hours",
                Frequencies.TwentyFourHours => "Every twenty four hours",
                _ => throw new NotImplementedException()
            },
            Language.Spanish => frequency switch
            {
                Frequencies.TwoHours => "Cada dos horas",
                Frequencies.FourHours => "Cada cuatro horas",
                Frequencies.EightHours => "Cada ocho horas",
                Frequencies.TwelveHours => "Cada doce horas",
                Frequencies.TwentyFourHours => "Cada veinticuatro horas",
                _ => throw new NotImplementedException()
            },
            Language.BrazilianPortuguese => frequency switch
            {
                Frequencies.TwoHours => "A cada duas horas",
                Frequencies.FourHours => "A cada quatro horas",
                Frequencies.EightHours => "A cada oito horas",
                Frequencies.TwelveHours => "A cada doze horas",
                Frequencies.TwentyFourHours => "A cada vinte e quatro horas",
                _ => throw new NotImplementedException()
            },
            _ => throw new NotImplementedException()
        };
    }
}