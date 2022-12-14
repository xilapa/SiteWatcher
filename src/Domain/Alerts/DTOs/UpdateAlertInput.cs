using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.DTOs;

public sealed class UpdateAlertInput
{
    public UpdateAlertInput(AlertId alertId, UpdateInfo<string>? name, UpdateInfo<Frequencies>? frequency,
        UpdateInfo<string>? siteName, UpdateInfo<string>? siteUri, UpdateInfo<Rules>? rule,
        UpdateInfo<string>? term, UpdateInfo<bool>? notifyOnDisappearance, UpdateInfo<string>? regexPattern
    )
    {
        AlertId = alertId;
        Name = name;
        Frequency = frequency;
        SiteName = siteName;
        SiteUri = siteUri;
        Rule = rule;
        Term = term;
        NotifyOnDisappearance = notifyOnDisappearance;
        RegexPattern = regexPattern;
    }

    public AlertId AlertId { get; set; }
    public UpdateInfo<string>? Name { get; set; }
    public UpdateInfo<Frequencies>? Frequency { get; set; }
    public UpdateInfo<string>? SiteName { get; set; }
    public UpdateInfo<string>? SiteUri { get; set; }
    public UpdateInfo<Rules>? Rule { get; set; }
    public UpdateInfo<string>? Term { get; set; }
    public UpdateInfo<bool>? NotifyOnDisappearance { get; set; }
    public UpdateInfo<string>? RegexPattern { get; set; }
}