namespace SiteWatcher.Application.Alerts.Commands.GetAlertDetails;

public class AlertDetails
{
    public string Id { get; set; } = null!;
    public string SiteUri { get; set; } = null!;
    public string WatchModeId { get; set; } = null!;
    public string? Term { get; set; }
}