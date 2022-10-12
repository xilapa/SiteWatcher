using SiteWatcher.Domain.Enums;

namespace Domain.DTOs.Alerts;

public class SimpleAlertViewDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public EFrequency Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    // TODO: Count notifications sent
    public int NotificationsSent { get; set; }
    public string SiteName { get; set; }
    public char WatchMode { get; set; }
}