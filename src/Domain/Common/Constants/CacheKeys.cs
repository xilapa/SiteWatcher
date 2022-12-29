using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Common.Constants;

public static class CacheKeys
{
    public static string InvalidUser(UserId userId) => $"InvalidUser_{userId}";
    public static string UserAlerts(UserId userId) => $"{AlertsKeyPrefix}_{userId}";
    public static string UserAlertSearch(UserId userId) => $"{AlertsKeyPrefix}Search_{userId}";
    public static string AlertsKeyPrefix => "Alerts";
}