using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.Utils;

public static class CacheKeys
{
    public static string InvalidUser(UserId userId) => $"InvalidUser_{userId}";
    public static string UserAlerts(UserId userId) => $"Alerts_{userId}";
    public static string UserAlertSearch(UserId userId) => $"AlertSearch_{userId}";
}