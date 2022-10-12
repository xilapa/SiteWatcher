using Domain.DTOs.Alerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Alerts;

namespace SiteWatcher.Application.Common.Extensions;

public static class AlertExtensions
{
    public static DetailedAlertView ToDetailedAlertView(this Alert alert, IIdHasher idHasher)
    {
        var hashedId = idHasher.HashId(alert.Id.Value);
        return new DetailedAlertView(alert, hashedId);
    }
}