using Domain.DTOs.Alerts;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.Models.Alerts;

public static class AlertFactory
{
    public static Alert Create(CreateAlertInput inputModel, UserId userId, DateTime currentDate)
    {
        var site = new Site(inputModel.SiteUri, inputModel.SiteName);
        var watchMode = CreateWatchMode(inputModel, currentDate);
        var alert = new Alert(userId, inputModel.Name, inputModel.Frequency, currentDate, site, watchMode);
        return alert;
    }

    private static WatchMode CreateWatchMode(CreateAlertInput inputModel, DateTime currentDate)
    {
        return inputModel.WatchMode switch
        {
            EWatchMode.AnyChanges => new AnyChangesWatch(currentDate),
            EWatchMode.Term => new TermWatch(inputModel.Term!, currentDate),
            _ => throw new ArgumentOutOfRangeException(nameof(inputModel.WatchMode))
        };
    }

    public static WatchMode CreateWatchMode(UpdateAlertInput updateInput, DateTime currentDate)
    {
        return updateInput.WatchMode.NewValue! switch
        {
            EWatchMode.AnyChanges => new AnyChangesWatch(currentDate),
            EWatchMode.Term => new TermWatch(updateInput.Term.NewValue!, currentDate),
            _ => throw new ArgumentOutOfRangeException(nameof(updateInput.WatchMode))
        };
    }
}