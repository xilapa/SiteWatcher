using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.Entities.Rules;

public abstract class Rule : BaseModel<RuleId>
{
    // ctor for EF
    protected Rule()
    { }

    protected Rule(DateTime currentDate) : base(new RuleId(), currentDate)
    {
        FirstWatchDone = false;
    }

    public bool FirstWatchDone { get; protected set; }

    /// <summary>
    /// Returns true if has detected changes
    /// </summary>
    /// <param name="html">The hmtl webpage as stream</param>
    /// <returns></returns>
    public abstract Task<bool> Execute(Stream html);
}