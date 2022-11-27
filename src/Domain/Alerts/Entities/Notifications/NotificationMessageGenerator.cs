using Fluid;
using SiteWatcher.Domain.Alerts.ValueObjects;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Utils.Notifications;

namespace SiteWatcher.Domain.Alerts.Entities.Notifications;

public static class NotificationMessageGenerator
{
    private static readonly FluidParser _parser = new();

    public static string GetSubject(Language language) =>
         LocalizedMessages.AlertNotificationMessageSubject(language);

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