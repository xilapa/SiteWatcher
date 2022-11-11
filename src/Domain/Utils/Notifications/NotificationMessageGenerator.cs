using Fluid;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Alerts;

namespace SiteWatcher.Domain.Utils.Notifications;

public static class NotificationMessageGenerator
{
    private static readonly FluidParser _parser = new();

    public static string GetSubject()
    {
        // TODO: return the subject based on user language
        return string.Empty;
    }

    public static async Task<string> GetBody(User user, List<AlertToNotify> successes, List<AlertToNotify> errors, string siteWatcherUri)
    {
        // User, Sucessos, Erros, URI -> Retorna o corpo em html pronto
        // Get the message template and the data to fill the template
        var messageTemplate = LocalizedMessages.AlertNotificationMessageTemplate(user.Language);
        var messageData = new AlertNotificationMessageData(
            user.Name, successes,
            errors, siteWatcherUri
            );

        // Fill the template using Fluid
        var parsedTemplate = _parser.Parse(messageTemplate);
        var templateOptions = new TemplateOptions();
        templateOptions.MemberAccessStrategy.Register<AlertToNotify>();
        var templateContext = new TemplateContext(messageData, templateOptions);
        var messageBody = await parsedTemplate.RenderAsync(templateContext);
        return messageBody;
    }
}