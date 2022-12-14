using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;
using static SiteWatcher.Domain.Common.Utils;

namespace SiteWatcher.Domain.Alerts.ValueObjects;

// TODO: Try to remove this
public sealed class AlertToNotify
{
    public AlertToNotify(Alert alert, NotificationId notificationId, NotificationType type, Language userLanguage)
    {
        Name = alert.Name;
        SiteUri = alert.Site.Uri.ToString();
        SiteName = alert.Site.Name;
        FrequencyString = LocalizedMessages.FrequencyString(userLanguage, alert.Frequency);
        var rule = GetRuleEnumByType(alert.Rule);
        RuleString = LocalizedMessages.RuleString(userLanguage, rule!.Value);
        Type = type;
        NotificationId = notificationId;
    }

    public string Name { get; }
    public string SiteUri { get; }
    public string SiteName { get; }
    public string FrequencyString { get; }
    public string RuleString { get; }
    public NotificationId NotificationId { get; }
    public NotificationType Type { get; set; }
}

public enum NotificationType
{
    Sucess = 1,
    Error
}