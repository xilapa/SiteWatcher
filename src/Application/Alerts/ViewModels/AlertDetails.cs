using Domain.DTOs.Alerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Alerts.WatchModes;

namespace SiteWatcher.Application.Alerts.ViewModels;

public class AlertDetails
{
    private AlertDetails(AlertDetailsDto alertDetailsDto, IIdHasher idHasher)
    {
        Id = idHasher.HashId(alertDetailsDto.Id);
        SiteUri = alertDetailsDto.SiteUri;
        WatchModeId = idHasher.HashId(alertDetailsDto.WatchModeId);
        Term = alertDetailsDto.Term;
        RegexPattern = alertDetailsDto.RegexPattern;
        NotifyOnDisappearance = alertDetailsDto.NotifyOnDisappearance;
    }

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

    public static AlertDetails FromDto(AlertDetailsDto dto, IIdHasher idHasher) =>
        new(dto, idHasher);

    public static AlertDetails FromAlert(Alert alert, IIdHasher idHasher) =>
        new(alert, idHasher);
}