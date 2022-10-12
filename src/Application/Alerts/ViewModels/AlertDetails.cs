using Domain.DTOs.Alerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Alerts.WatchModes;

namespace SiteWatcher.Application.Alerts.ViewModels;

public class AlertDetails
{
    public AlertDetails()
    { }

    public AlertDetails(AlertDetailsDto alertDetailsDto, IIdHasher idHasher)
    {
        Id = idHasher.HashId(alertDetailsDto.Id);
        SiteUri = alertDetailsDto.SiteUri;
        WatchModeId = idHasher.HashId(alertDetailsDto.WatchModeId);
        Term = alertDetailsDto.Term;
    }

    public AlertDetails(Alert alert, IIdHasher idHasher)
    {
        Id = idHasher.HashId(alert.Id.Value);
        SiteUri = alert.Site.Uri.ToString();
        WatchModeId = idHasher.HashId(alert.WatchMode.Id.Value);
        Term = (alert.WatchMode as TermWatch)?.Term;
    }

    public string Id { get; set; } = null!;
    public string SiteUri { get; set; } = null!;
    public string WatchModeId { get; set; } = null!;
    public string? Term { get; set; }

    public static AlertDetails FromDto(AlertDetailsDto dto, IIdHasher idHasher) =>
        new(dto, idHasher);

    public static AlertDetails FromAlert(Alert alert, IIdHasher idHasher) =>
        new(alert, idHasher);
}