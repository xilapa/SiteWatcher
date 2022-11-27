using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.Entities.WatchModes;

public abstract class WatchMode : BaseModel<WatchModeId>
{
    // ctor for EF
    protected WatchMode()
    { }

    protected WatchMode(DateTime currentDate) : base(new WatchModeId(), currentDate)
    {
        FirstWatchDone = false;
    }

    public bool FirstWatchDone { get; protected set; }

    /// <summary>
    /// Returns true if has detected changes
    /// </summary>
    /// <param name="html">The hmtl webpage as stream</param>
    /// <returns></returns>
    public abstract Task<bool> VerifySite(Stream html);
}