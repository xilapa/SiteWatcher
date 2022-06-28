using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Utils;

namespace Domain.DTOs.Alert;

public class DetailedWatchModeView
{
    public string Id { get; set; }
    public EWatchMode? WatchMode { get; set; }
    public string? Term { get; set; }

    public static DetailedWatchModeView FromModel(WatchMode watchMode)
    {
        var watchModeType = Utils.GetWatchModeEnumByType(watchMode);
        var detailedView = new DetailedWatchModeView
        {
            WatchMode = watchModeType,
            Term = watchModeType == EWatchMode.Term ? (watchMode as TermWatch)!.Term : null
        };
        return detailedView;
    }
}