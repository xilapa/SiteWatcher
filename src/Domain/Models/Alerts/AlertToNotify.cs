using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Utils;
using static SiteWatcher.Domain.Utils.Utils;

namespace SiteWatcher.Domain.Models.Alerts;

public class AlertToNotify
{
    public AlertToNotify(Alert alert, ELanguage userLanguage)
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