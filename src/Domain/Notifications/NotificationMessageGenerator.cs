using System.Collections.Frozen;
using Fluid;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Notifications;

public static class NotificationMessageGenerator
{
    private static readonly FrozenDictionary<Language, IFluidTemplate> _parsedTemplates;

    static NotificationMessageGenerator()
    {
        _parsedTemplates = ParseTemplates();
    }

    private static FrozenDictionary<Language, IFluidTemplate> ParseTemplates()
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
        Console.WriteLine("Fluid Email Templates Parsed");
        return parsedTemplatesMutableDict.ToFrozenDictionary();
    }

    public static string GetSubject(NotificationData notificationData) =>
        LocalizedMessages.AlertNotificationMessageSubject(notificationData.Language);

    public static ValueTask<string> GetBody(NotificationData notificationData)
    {
        var templateOptions = new TemplateOptions();
        templateOptions.MemberAccessStrategy.Register<NotificationData>();
        templateOptions.MemberAccessStrategy.Register<AlertData>();
        var templateContext = new TemplateContext(notificationData, templateOptions);
        var parsedTemplate = _parsedTemplates[notificationData.Language];
        return parsedTemplate.RenderAsync(templateContext);
    }
}