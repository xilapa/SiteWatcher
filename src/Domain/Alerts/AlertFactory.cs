using Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Entities.WatchModes;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.ValueObjects;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts;

public static class AlertFactory
{
    public static Alert Create(CreateAlertInput inputModel, UserId userId, DateTime currentDate)
    {
        var site = new Site(inputModel.SiteUri, inputModel.SiteName);
        var watchMode = CreateWatchMode(inputModel, currentDate);
        return new Alert(userId, inputModel.Name, inputModel.Frequency, currentDate, site, watchMode);
    }

    private static WatchMode CreateWatchMode(CreateAlertInput inputModel, DateTime currentDate)
    {
        return inputModel.WatchMode switch
        {
            WatchModes.AnyChanges => new AnyChangesWatch(currentDate),
            WatchModes.Term => new TermWatch(inputModel.Term!, currentDate),
            WatchModes.Regex => new RegexWatch(inputModel.RegexPattern!,
                inputModel.NotifyOnDisappearance!.Value, currentDate),
            _ => throw new ArgumentOutOfRangeException(nameof(inputModel.WatchMode))
        };
    }

    public static WatchMode CreateWatchMode(UpdateAlertInput updateInput, DateTime currentDate)
    {
        return updateInput.WatchMode!.NewValue! switch
        {
            WatchModes.AnyChanges => new AnyChangesWatch(currentDate),
            WatchModes.Term => new TermWatch(updateInput.Term!.NewValue!, currentDate),
            WatchModes.Regex => new RegexWatch(updateInput.RegexPattern!.NewValue!,
                updateInput.NotifyOnDisappearance!.NewValue, currentDate),
            _ => throw new ArgumentOutOfRangeException(nameof(updateInput.WatchMode))
        };
    }
}