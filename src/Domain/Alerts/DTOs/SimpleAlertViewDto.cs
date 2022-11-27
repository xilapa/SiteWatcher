using SiteWatcher.Domain.Alerts.Enums;
namespace Domain.Alerts.DTOs;

public sealed class SimpleAlertViewDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public Frequencies Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    // TODO: Count notifications sent
    public int NotificationsSent { get; set; }
    public string? SiteName { get; set; }
    public char WatchMode { get; set; }
}