using SiteWatcher.Domain.Alerts.Entities.Triggerings;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using static SiteWatcher.Domain.Common.Utils;

namespace SiteWatcher.Domain.Alerts.Events;

public sealed class AlertsTriggeredEvent : BaseEvent
{
    public AlertsTriggeredEvent(User user, IEnumerable<AlertTriggered> alertsTriggered)
    {
        UserName = user.Name;
        UserLanguage = user.Language;
        Alerts = alertsTriggered;
    }

    public string UserName { get; }
    public Language UserLanguage { get; }
    public IEnumerable<AlertTriggered> Alerts { get; }
}

public sealed class AlertTriggered
{
    public AlertTriggered(Alert alert, TriggeringStatus status, DateTime triggeringDate)
    {
        AlertId = alert.Id;
        AlertName = alert.Name;
        SiteUri = alert.Site.Uri.ToString();
        SiteName = alert.Site.Name;
        Frequency = alert.Frequency;
        Rule = GetRuleEnumByType(alert.Rule)!.Value;
        Status = status;
        TriggeringDate = triggeringDate;
    }

    public AlertId AlertId { get; set; }
    public string AlertName { get; }
    public string SiteUri { get; }
    public string SiteName { get; }
    public Frequencies Frequency { get; }
    public Rules Rule { get; }
    public TriggeringStatus Status { get; }
    public DateTime TriggeringDate { get; set; }
}