using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Alerts.Commands.GetUserAlerts;

public class SimpleAlertView
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public EFrequency Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    // TODO: Count notifications sent
    public int NotificationsSent { get; set; }
    public string SiteName { get; set; }
    public EWatchMode WatchMode { get; set; }
}