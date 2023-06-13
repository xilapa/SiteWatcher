using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.DTOs;

public sealed class UpdateAlertDto
{
    public AlertId Id { get; set; }
    public UserId UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastVerification { get; set; }
    public string Name { get; set; } = null!;
    public Frequencies Frequency { get; set; }
    public string SiteName { get; set; } = null!;
    public Uri SiteUri { get; set; } = null!;
    public Rule Rule { get; set; } = null!;
}