using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Models.Common;

namespace Domain.DTOs.Alert;

public class UpdateAlertDto
{
    public AlertId Id { get; set; }
    public UserId UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; }
    public EFrequency Frequency { get; set; }
    public string SiteName { get; set; }
    public Uri SiteUri { get; set; }
    public WatchMode WatchMode { get; set; }
}