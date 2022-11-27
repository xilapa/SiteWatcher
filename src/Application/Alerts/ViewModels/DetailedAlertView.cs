using Domain.Alerts.DTOs;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Application.Alerts.ViewModels;

public class DetailedAlertView
{
    public DetailedAlertView(string id, string name, DateTime createdAt, Frequencies frequency,
        DateTime? lastVerification, int notificationsSent, SiteView site, DetailedWatchModeView watchMode)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
        Frequency = frequency;
        LastVerification = lastVerification;
        NotificationsSent = notificationsSent;
        Site = site;
        WatchMode = watchMode;
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
        WatchMode = alert.WatchMode;
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
    public DetailedWatchModeView? WatchMode { get; set; }

    public static DetailedAlertView FromAlert(Alert alert, IIdHasher idHasher) =>
        new(alert, idHasher);
}