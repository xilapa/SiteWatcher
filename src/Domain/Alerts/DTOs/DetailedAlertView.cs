using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Domain.Alerts.DTOs;

public class DetailedAlertView
{
    public DetailedAlertView(string id, string name, DateTime createdAt, Frequencies frequency,
        DateTime? lastVerification, int notificationsSent, SiteView site, DetailedRuleView rule)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
        Frequency = frequency;
        LastVerification = lastVerification;
        NotificationsSent = notificationsSent;
        Site = site;
        Rule = rule;
    }

    public DetailedAlertView(Alert alert, IIdHasher idHasher)
    {
        Id = idHasher.HashId(alert.Id.Value);
        Name = alert.Name;
        CreatedAt = alert.CreatedAt;
        Frequency = alert.Frequency;
        LastVerification = alert.LastVerification;
        NotificationsSent = 0;
        Site = alert.Site;
        Rule = alert.Rule;
    }

    public DetailedAlertView()
    { }

    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public Frequencies Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    public int NotificationsSent { get; set; }
    public SiteView? Site { get; set; }
    public DetailedRuleView? Rule { get; set; }

    public static DetailedAlertView FromAlert(Alert alert, IIdHasher idHasher) =>
        new(alert, idHasher);
}