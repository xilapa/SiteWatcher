using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Utils;

namespace Domain.DTOs.Alerts;

public class DetailedWatchModeView
{
    public EWatchMode? WatchMode { get; set; }
    public string? Term { get; set; }
    public string? RegexPattern { get; set; }
    public bool? NotifyOnDisappearance { get; set; }

    // TODO: remove this operator
    public static implicit operator DetailedWatchModeView(WatchMode watchMode)
    {
        var watchModeType = Utils.GetWatchModeEnumByType(watchMode);
        var detailedView = new DetailedWatchModeView
        {
            WatchMode = watchModeType,
            Term = watchModeType == EWatchMode.Term ? (watchMode as TermWatch)!.Term : null,
            RegexPattern = watchModeType == EWatchMode.Regex ? (watchMode as RegexWatch)!.RegexPattern : null,
            NotifyOnDisappearance = watchModeType == EWatchMode.Regex ? (watchMode as RegexWatch)!.NotifyOnDisappearance : null
        };
        return detailedView;
    }
}