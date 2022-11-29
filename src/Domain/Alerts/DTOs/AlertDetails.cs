using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Entities.WatchModes;

namespace SiteWatcher.Domain.Alerts.DTOs;

public class AlertDetails
{
    public AlertDetails()
    { }

    private AlertDetails(Alert alert, IIdHasher idHasher)
    {
        Id = idHasher.HashId(alert.Id.Value);
        SiteUri = alert.Site.Uri.ToString();
        WatchModeId = idHasher.HashId(alert.WatchMode.Id.Value);
        Term = (alert.WatchMode as TermWatch)?.Term;
        RegexPattern = (alert.WatchMode as RegexWatch)?.RegexPattern;
        NotifyOnDisappearance = (alert.WatchMode as RegexWatch)?.NotifyOnDisappearance;
    }

    public string Id { get; set; } = null!;
    public string SiteUri { get; set; } = null!;
    public string WatchModeId { get; set; } = null!;
    public string? Term { get; set; }
    public string? RegexPattern { get; set; }
    public bool? NotifyOnDisappearance { get; set; }

    public static AlertDetails FromAlert(Alert alert, IIdHasher idHasher) =>
        new(alert, idHasher);
}