using SiteWatcher.Domain.Alerts.Entities.WatchModes;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.ValueObjects;

namespace Domain.Alerts.DTOs;

public sealed class UpdateAlertDto
{
    public AlertId Id { get; set; }
    public UserId UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; } = null!;
    public Frequencies Frequency { get; set; }
    public string SiteName { get; set; } = null!;
    public Uri SiteUri { get; set; } = null!;
    public WatchMode WatchMode { get; set; } = null!;
}