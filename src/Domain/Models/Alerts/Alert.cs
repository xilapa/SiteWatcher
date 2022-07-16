﻿using Domain.DTOs.Alert;
using Domain.Events.Alerts;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.Models.Alerts;

public class Alert : BaseModel<AlertId>
{
    // ctor for EF
    protected Alert() : base()
    { }

    public Alert(UserId userId, string name, EFrequency frequency, DateTime currentDate, Site site, WatchMode watchMode)
        : base(new AlertId(), currentDate)
    {
        UserId = userId;
        Name = name;
        Frequency = frequency;
        Site = site;
        WatchMode = watchMode;
        AddDomainEvent(new AlertsChangedEvent(UserId));
    }

    public UserId UserId { get; private set; }
    public string Name { get; private set; }
    public EFrequency Frequency { get; private set; }
    public DateTime? LastVerification { get; private set; }
    public Site Site { get; private set; }

    public WatchMode WatchMode { get; private set; }

    public static Alert GetModelForUpdate(UpdateAlertDto updateAlertDto) =>
        new()
        {
            Id = updateAlertDto.Id,
            UserId = updateAlertDto.UserId,
            CreatedAt = updateAlertDto.CreatedAt,
            Name = updateAlertDto.Name,
            Frequency = updateAlertDto.Frequency,
            Site = new Site(updateAlertDto.SiteUri, updateAlertDto.Name)
        };

    public void Update(UpdateAlertInput updateInput, DateTime updateDate)
    {
        if (updateInput.Name is not null)
            Name = updateInput.Name.NewValue!;

        if (updateInput.Frequency is not null)
            Frequency = updateInput.Frequency.NewValue;

        if (updateInput.SiteName is not null || updateInput.SiteUri is not null)
            UpdateSite(updateInput);

        if (updateInput.WatchMode is not null || updateInput.Term is not null)
            UpdateWatchMode(updateInput, updateDate);

        LastUpdatedAt = updateDate;

        AddDomainEvent(new AlertsChangedEvent(UserId));
    }

    private void UpdateSite(UpdateAlertInput updateInput)
    {
        var newSiteName = Site.Name;
        var newSiteUri = Site.Uri.AbsoluteUri;

        if (updateInput.SiteName is not null)
            newSiteName = updateInput.SiteName.NewValue;

        if (updateInput.SiteUri is not null)
            newSiteUri = updateInput.SiteUri.NewValue;

        Site = new Site(newSiteUri!, newSiteName!);
    }

    private void UpdateWatchMode(UpdateAlertInput updateInput, DateTime updateDate)
    {
        var currentWatchMode = Utils.Utils.GetWatchModeEnumByType(WatchMode);

        // If the current watch mode is really new, then recreate it
        if (updateInput.WatchMode is not null && !currentWatchMode!.Value.Equals(updateInput.WatchMode.NewValue))
        {
            // Create the event before the WatchMode's Id is set to zero
            AddDomainEvent(new AlertWatchModeChangedEvent(WatchMode.Id));
            WatchMode = AlertFactory.CreateWatchMode(updateInput, updateDate);
            return;
        }

        // Update term watchMode
        if (currentWatchMode.Equals(EWatchMode.Term) && updateInput.Term is not null)
            (WatchMode as TermWatch)!.Update(updateInput);

        // any changes doesn't have field to update, so just ignore it
    }
}