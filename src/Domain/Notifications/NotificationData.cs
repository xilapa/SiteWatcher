using SiteWatcher.Domain.Alerts.Entities.Triggerings;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Notifications;

public sealed class NotificationData
{
    public NotificationData(string userName, Language language, string email, string siteWatcherUri)
    {
        UserName = userName;
        Language = language;
        Email = email;
        SiteWatcherUri = siteWatcherUri;
        _successes = new List<AlertData>();
        _errors = new List<AlertData>();
    }

    public string UserName { get; }
    public Language Language { get; }
    public string Email { get; set; }
    public string SiteWatcherUri { get; }
    private readonly List<AlertData> _successes;
    public IReadOnlyCollection<AlertData> AlertSuccess => _successes.AsReadOnly();
    public bool HasSuccess => _successes.Count > 0;
    private readonly List<AlertData> _errors;
    public IReadOnlyCollection<AlertData> AlertErrors => _errors.AsReadOnly();
    public bool HasErrors => _errors.Count > 0;

    public void AddAlertTriggered(AlertTriggered alertTriggered)
    {
        if (TriggeringStatus.Success.Equals(alertTriggered.Status))
            _successes.Add(new AlertData(alertTriggered, Language));
        if (TriggeringStatus.Error.Equals(alertTriggered.Status))
            _errors.Add(new AlertData(alertTriggered, Language));
    }
}

public sealed class AlertData
{
    public AlertData(AlertTriggered alertTriggered, Language language)
    {
        Name = alertTriggered.AlertName;
        SiteUri = alertTriggered.SiteUri;
        SiteName = alertTriggered.SiteName;
        Frequency = LocalizedMessages.FrequencyString(language, alertTriggered.Frequency);
        Rule = LocalizedMessages.RuleString(language, alertTriggered.Rule);
    }

    public string Name { get; }
    public string SiteUri { get; }
    public string SiteName { get; }
    public string Frequency { get; }
    public string Rule { get; }
}