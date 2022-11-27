using SiteWatcher.Domain.Alerts.Entities.WatchModes;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common;

namespace Domain.Alerts.DTOs;

public sealed class DetailedWatchModeView
{
    public WatchModes? WatchMode { get; set; }
    public string? Term { get; set; }
    public string? RegexPattern { get; set; }
    public bool? NotifyOnDisappearance { get; set; }

    // TODO: remove this operator
    public static implicit operator DetailedWatchModeView(WatchMode watchMode)
    {
        var watchModeType = Utils.GetWatchModeEnumByType(watchMode);
        return new DetailedWatchModeView
        {
            WatchMode = watchModeType,
            Term = watchModeType == WatchModes.Term ? (watchMode as TermWatch)!.Term : null,
            RegexPattern = watchModeType == WatchModes.Regex ? (watchMode as RegexWatch)!.RegexPattern : null,
            NotifyOnDisappearance = watchModeType == WatchModes.Regex ? (watchMode as RegexWatch)!.NotifyOnDisappearance : null
        };
    }
}