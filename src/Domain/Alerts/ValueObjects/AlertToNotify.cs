using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;
using static SiteWatcher.Domain.Common.Utils;

namespace SiteWatcher.Domain.Alerts.ValueObjects;

// TODO: Try to remove this
public sealed class AlertToNotify
{
    public AlertToNotify(Alert alert, NotificationId notificationId, Language userLanguage)
    {
        Name = alert.Name;
        SiteUri = alert.Site.Uri.ToString();
        SiteName = alert.Site.Name;
        FrequencyString = LocalizedMessages.FrequencyString(userLanguage, alert.Frequency);
        var watchMode = GetWatchModeEnumByType(alert.WatchMode);
        WatchModeString = LocalizedMessages.WatchModeString(userLanguage, watchMode!.Value);
        NotificationId = notificationId;
    }

    public string Name { get; }
    public string SiteUri { get; }
    public string SiteName { get; }
    public string FrequencyString { get; }
    public string WatchModeString { get; }
    public NotificationId NotificationId { get; }
}