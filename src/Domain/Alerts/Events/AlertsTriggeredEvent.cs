using SiteWatcher.Domain.Alerts.Entities.Triggerings;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using static SiteWatcher.Domain.Common.Utils;

namespace SiteWatcher.Domain.Alerts.Events;

public class AlertsTriggeredEvent : BaseEvent
{
    public AlertsTriggeredEvent(){}

    public AlertsTriggeredEvent(User user, IEnumerable<AlertTriggered> alertsTriggered, DateTime currentDate)
    {
        UserId = user.Id;
        UserName = user.Name;
        UserLanguage = user.Language;
        UserEmail = user.Email;
        Alerts = alertsTriggered;
        Id = $"U{UserId}-{currentDate.Ticks}";
    }

    public string Id { get; set; }
    public UserId UserId { get; set; }
    public string UserName { get; set; }
    public Language UserLanguage { get; set; }
    public string UserEmail { get; set; }
    public IEnumerable<AlertTriggered> Alerts { get; set; }
}

public class AlertTriggered
{
    public AlertTriggered() {}

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
    public string AlertName { get; set; }
    public string SiteUri { get; set; }
    public string SiteName { get; set; }
    public Frequencies Frequency { get; set; }
    public Rules Rule { get; set; }
    public TriggeringStatus Status { get; set; }
    public DateTime TriggeringDate { get; set; }
}