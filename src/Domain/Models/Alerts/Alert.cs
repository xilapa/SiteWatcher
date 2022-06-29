using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.Models.Alerts;

public class Alert : BaseModel<AlertId>
{
    // ctor for EF
    protected Alert() : base()
    { }

    public Alert(UserId userId, string name, EFrequency frequency, DateTime currentDate, Site site, WatchMode watchMode)
        : base(new AlertId(), currentDate)
    {
        UserId = userId;
        Name = name;
        Frequency = frequency;
        Site = site;
        WatchMode = watchMode;
    }

    public UserId UserId { get; }
    public string Name { get; private set; }
    public EFrequency Frequency { get; private set; }
    public DateTime? LastVerification { get; private set; }
    public Site Site { get; private set; }
    public WatchMode WatchMode { get; private set; }
}