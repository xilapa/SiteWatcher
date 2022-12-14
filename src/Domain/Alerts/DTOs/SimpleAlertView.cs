using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common;

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
        NotificationsSent = 0;
        SiteName = alert.Site.Name;
        Rule = Utils.GetRuleEnumByType(alert.Rule)!.Value;
    }

    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public Frequencies Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    // TODO: Count notifications sent
    public int NotificationsSent { get; set; }
    public string? SiteName { get; set; }
    public Rules Rule { get; set; }

    public static SimpleAlertView FromAlert(Alert alert, IIdHasher idHasher) =>
        new(alert, idHasher);
}