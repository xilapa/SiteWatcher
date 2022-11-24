using Domain.DTOs.Alerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.ViewModels;

public class SimpleAlertView
{
    public SimpleAlertView()
    { }

    public SimpleAlertView(SimpleAlertViewDto alertViewDto, IIdHasher idHasher)
    {
        Id = idHasher.HashId(alertViewDto.Id);
        Name = alertViewDto.Name;
        CreatedAt = alertViewDto.CreatedAt;
        Frequency = alertViewDto.Frequency;
        LastVerification = alertViewDto.LastVerification;
        NotificationsSent = 0;
        SiteName = alertViewDto.SiteName;
        WatchMode = Utils.GetWatchModeEnumByTableDiscriminator(alertViewDto.WatchMode)!.Value;
    }

    public SimpleAlertView(Alert alert, IIdHasher idHasher)
    {
        Id = idHasher.HashId(alert.Id.Value);
        Name = alert.Name;
        CreatedAt = alert.CreatedAt;
        Frequency = alert.Frequency;
        LastVerification = alert.LastVerification;
        NotificationsSent = 0;
        SiteName = alert.Site.Name;
        WatchMode = Utils.GetWatchModeEnumByType(alert.WatchMode)!.Value;
    }

    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public EFrequency Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    // TODO: Count notifications sent
    public int NotificationsSent { get; set; }
    public string? SiteName { get; set; }
    public EWatchMode WatchMode { get; set; }

    public static SimpleAlertView FromDto(SimpleAlertViewDto dto, IIdHasher idHasher) =>
        new(dto, idHasher);

    public static SimpleAlertView FromAlert(Alert alert, IIdHasher idHasher) =>
        new(alert, idHasher);
}