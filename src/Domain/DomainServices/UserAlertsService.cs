using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Messages;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Domain.DomainServices;

public sealed class UserAlertsService
{
    private readonly IHttpClient _httpClient;

    public UserAlertsService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Executes all user alerts. If there are no notification for the user, null is returned.
    /// </summary>
    /// <param name="user">User with the alerts and rules loaded</param>
    /// <param name="currentTime">Current time</param>
    /// <param name="ct">Cancellation token</param>
    public async Task<AlertExecutionStatus> ExecuteAlerts(User user, DateTime currentTime, CancellationToken ct)
    {
        var alertsTriggered = new List<AlertTriggered>();
        var errors = new List<AlertExecutionError>();
        foreach (var alert in user.Alerts)
        {
            try
            {
                var htmlStream = await _httpClient.GetStreamAsync(alert.Site.Uri, ct);
                var alertTriggered = await alert.ExecuteRule(htmlStream, currentTime);
                if (alertTriggered != null)
                    alertsTriggered.Add(alertTriggered);
            }
            catch (Exception e)
            {
                errors.Add(new AlertExecutionError { AlertId = alert.Id, Exception = e });
            }
        }

        return new AlertExecutionStatus(new AlertsTriggeredMessage(user, alertsTriggered, currentTime), errors);
    }
}

public sealed class AlertExecutionStatus
{
    public AlertExecutionStatus(AlertsTriggeredMessage alertsTriggered, List<AlertExecutionError> errors)
    {
        AlertsTriggered = alertsTriggered;
        Errors = errors;
    }

    public AlertsTriggeredMessage AlertsTriggered { get; }
    public List<AlertExecutionError> Errors { get; }
}

public sealed class AlertExecutionError
{
    public AlertId AlertId { get; set; }
    public Exception Exception { get; set; } = null!;
}