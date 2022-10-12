﻿using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts;

namespace Domain.DTOs.Alerts;

public class DetailedAlertView
{
    public DetailedAlertView(string id, string name, DateTime createdAt, EFrequency frequency, DateTime? lastVerification, int notificationsSent, SiteView site, DetailedWatchModeView watchMode)
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

    public DetailedAlertView(Alert alert, string hashedId)
    {
        Id = hashedId;
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

    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public EFrequency Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    public int NotificationsSent { get; set; }
    public SiteView Site { get; set; }
    public DetailedWatchModeView WatchMode { get; set; }
}