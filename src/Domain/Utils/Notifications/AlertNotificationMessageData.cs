using SiteWatcher.Domain.Models.Alerts;

namespace SiteWatcher.Domain.Utils.Notifications;

public class AlertNotificationMessageData
{
    public AlertNotificationMessageData(string userName, List<AlertToNotify> successNotifications,
        List<AlertToNotify> errorNotifications, string siteWatcherUri)
    {
        UserName = userName;
        AlertsToNotifySuccess = successNotifications;
        AlertsToNotifyError = errorNotifications;
        SiteWatcherUri = siteWatcherUri;
    }

    public string UserName { get; }
    public bool HasSuccess => AlertsToNotifySuccess.Count > 0;
    public List<AlertToNotify> AlertsToNotifySuccess { get; }
    public bool HasErrors => AlertsToNotifyError.Count > 0;
    public List<AlertToNotify> AlertsToNotifyError { get; }
    public string SiteWatcherUri { get; set; }
}