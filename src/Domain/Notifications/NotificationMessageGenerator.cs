using System.Collections.ObjectModel;
using Fluid;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Notifications;

public static class NotificationMessageGenerator
{
    private static readonly Lazy<ReadOnlyDictionary<Language, IFluidTemplate>> _parsedTemplates;

    static NotificationMessageGenerator()
    {
        _parsedTemplates = new Lazy<ReadOnlyDictionary<Language, IFluidTemplate>>(ParseTemplates);
    }

    private static ReadOnlyDictionary<Language, IFluidTemplate> ParseTemplates()
    {
        // Parse templates
        var fluidParser = new FluidParser();
        var parsedTemplatesMutableDict = new Dictionary<Language, IFluidTemplate>();
        foreach (var lang in Enum.GetValues<Language>())
        {
            var rawTemplate = LocalizedMessages.AlertNotificationMessageTemplate(lang);
            var parsedTemplate = fluidParser.Parse(rawTemplate);
            parsedTemplatesMutableDict.Add(lang, parsedTemplate);
        }
        return new ReadOnlyDictionary<Language, IFluidTemplate>(parsedTemplatesMutableDict);
    }

    public static string GetSubject(NotificationData notificationData) =>
        LocalizedMessages.AlertNotificationMessageSubject(notificationData.Language);

    public static async Task<string> GetBody(NotificationData notificationData)
    {
        var templateOptions = new TemplateOptions();
        templateOptions.MemberAccessStrategy.Register<NotificationData>();
        var templateContext = new TemplateContext(notificationData, templateOptions);
        var parsedTemplate = _parsedTemplates.Value[notificationData.Language];
        return await parsedTemplate.RenderAsync(templateContext);
    }
}