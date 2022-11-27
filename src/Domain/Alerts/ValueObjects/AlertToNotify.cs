using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Users.Enums;
using static SiteWatcher.Domain.Common.Utils;

namespace SiteWatcher.Domain.Alerts.ValueObjects;

// TODO: Remove this
public sealed class AlertToNotify
{
    public AlertToNotify(Alert alert, Language userLanguage)
    {
        Name = alert.Name;
        SiteUri = alert.Site.Uri.ToString();
        SiteName = alert.Site.Name;
        FrequencyString = LocalizedMessages.FrequencyString(userLanguage, alert.Frequency);
        var watchMode = GetWatchModeEnumByType(alert.WatchMode);
        WatchModeString = LocalizedMessages.WatchModeString(userLanguage, watchMode!.Value);
    }

    public string Name { get; private set; }
    public string SiteUri { get; private set; }
    public string SiteName { get; private set; }
    public string FrequencyString { get; private set; }
    public string WatchModeString { get; private set; }

}