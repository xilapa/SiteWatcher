using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.Models.Alerts.WatchModes;

public abstract class WatchMode : BaseModel<WatchModeId>
{
    // ctor for EF
    protected WatchMode()
    { }

    protected WatchMode(DateTime currentDate) : base(new WatchModeId(), currentDate)
    {
        FirstWatchDone = false;
    }

    public AlertId AlertId { get; set; }
    public bool FirstWatchDone { get; protected set; }
}