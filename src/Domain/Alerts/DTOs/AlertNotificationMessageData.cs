using SiteWatcher.Domain.Alerts.ValueObjects;

namespace SiteWatcher.Domain.Alerts.DTOs;

// TODO: this needs to be near the notification
public sealed class AlertNotificationMessageData
{
    public AlertNotificationMessageData(string userName, IEnumerable<AlertToNotify> successNotifications,
        IEnumerable<AlertToNotify> errorNotifications, string siteWatcherUri)
    {
        UserName = userName;
        AlertsToNotifySuccess = successNotifications.ToArray();
        AlertsToNotifyError = errorNotifications.ToArray();
        SiteWatcherUri = siteWatcherUri;
    }

    public string UserName { get; }
    public bool HasSuccess => AlertsToNotifySuccess.Length > 0;
    public AlertToNotify[] AlertsToNotifySuccess { get; }
    public bool HasErrors => AlertsToNotifyError.Length > 0;
    public AlertToNotify[] AlertsToNotifyError { get; }
    public string SiteWatcherUri { get; set; }
}