using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Entities.Rules;

namespace SiteWatcher.Domain.Alerts.DTOs;

public class AlertDetails
{
    public AlertDetails()
    { }

    private AlertDetails(Alert alert, IIdHasher idHasher)
    {
        Id = idHasher.HashId(alert.Id.Value);
        SiteUri = alert.Site.Uri.ToString();
        RuleId = idHasher.HashId(alert.Rule.Id.Value);
        Term = (alert.Rule as TermRule)?.Term;
        RegexPattern = (alert.Rule as RegexRule)?.RegexPattern;
        NotifyOnDisappearance = (alert.Rule as RegexRule)?.NotifyOnDisappearance;
    }

    public string Id { get; set; } = null!;
    public string SiteUri { get; set; } = null!;
    public string RuleId { get; set; } = null!;
    public string? Term { get; set; }
    public string? RegexPattern { get; set; }
    public bool? NotifyOnDisappearance { get; set; }

    public static AlertDetails FromAlert(Alert alert, IIdHasher idHasher) =>
        new(alert, idHasher);
}