using SiteWatcher.Domain.Enums;

namespace Domain.DTOs.Alert;

public class DetailedAlertView
{
    public string Id { get; set; }
    public string Name { get; set; }
    public EFrequency Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    public int NotificationsSent { get; set; }
    public SiteView Site { get; set; }
    public DetailedWatchModeView WatchMode { get; set; }
}