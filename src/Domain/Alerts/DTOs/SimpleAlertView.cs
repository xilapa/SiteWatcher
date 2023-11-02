using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Domain.Alerts.DTOs;

public class SimpleAlertView
{
    public SimpleAlertView()
    { }

    public SimpleAlertView(Alert alert, IIdHasher idHasher)
    {
        Id = idHasher.HashId(alert.Id.Value);
        Name = alert.Name;
        CreatedAt = alert.CreatedAt;
        Frequency = alert.Frequency;
        LastVerification = alert.LastVerification;
        TriggeringsCount = alert.Triggerings.Count;
        SiteName = alert.Site.Name;
        RuleType = alert.Rule.RuleType;
    }

    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public Frequencies Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    public int TriggeringsCount { get; set; }
    public string? SiteName { get; set; }
    public RuleType RuleType { get; set; }

    public static SimpleAlertView FromAlert(Alert alert, IIdHasher idHasher) =>
        new(alert, idHasher);
}