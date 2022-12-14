using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.ValueObjects;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts;

public static class AlertFactory
{
    public static Alert Create(CreateAlertInput inputModel, UserId userId, DateTime currentDate)
    {
        var site = new Site(inputModel.SiteUri, inputModel.SiteName);
        var rule = CreateRule(inputModel, currentDate);
        return new Alert(userId, inputModel.Name, inputModel.Frequency, currentDate, site, rule);
    }

    private static Rule CreateRule(CreateAlertInput inputModel, DateTime currentDate)
    {
        return inputModel.Rule switch
        {
            Rules.AnyChanges => new AnyChangesRule(currentDate),
            Rules.Term => new TermRule(inputModel.Term!, currentDate),
            Rules.Regex => new RegexRule(inputModel.RegexPattern!,
                inputModel.NotifyOnDisappearance!.Value, currentDate),
            _ => throw new ArgumentOutOfRangeException(nameof(inputModel.Rule))
        };
    }

    public static Rule CreateRule(UpdateAlertInput updateInput, DateTime currentDate)
    {
        return updateInput.Rule!.NewValue! switch
        {
            Rules.AnyChanges => new AnyChangesRule(currentDate),
            Rules.Term => new TermRule(updateInput.Term!.NewValue!, currentDate),
            Rules.Regex => new RegexRule(updateInput.RegexPattern!.NewValue!,
                updateInput.NotifyOnDisappearance!.NewValue, currentDate),
            _ => throw new ArgumentOutOfRangeException(nameof(updateInput.Rule))
        };
    }
}