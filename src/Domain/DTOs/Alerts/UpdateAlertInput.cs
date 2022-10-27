﻿using Domain.DTOs.Common;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;

namespace Domain.DTOs.Alerts;

public class UpdateAlertInput
{
    public UpdateAlertInput(AlertId alertId, UpdateInfo<string>? name, UpdateInfo<EFrequency>? frequency, UpdateInfo<string>? siteName, UpdateInfo<string>? siteUri, UpdateInfo<EWatchMode>? watchMode, UpdateInfo<string>? term)
    {
        AlertId = alertId;
        Name = name;
        Frequency = frequency;
        SiteName = siteName;
        SiteUri = siteUri;
        WatchMode = watchMode;
        Term = term;
    }

    public AlertId AlertId { get; set; }
    public UpdateInfo<string>? Name { get; set; }
    public UpdateInfo<EFrequency>? Frequency { get; set; }
    public UpdateInfo<string>? SiteName { get; set; }
    public UpdateInfo<string>? SiteUri { get; set; }
    public UpdateInfo<EWatchMode>? WatchMode { get; set; }
    public UpdateInfo<string>? Term { get; set; }
}