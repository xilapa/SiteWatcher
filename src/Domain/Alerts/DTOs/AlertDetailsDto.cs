namespace Domain.Alerts.DTOs;

public sealed class AlertDetailsDto
{
    public int Id { get; set; }
    public string SiteUri { get; set; } = null!;
    public int WatchModeId { get; set; }
    public string? Term { get; set; }
    public bool? NotifyOnDisappearance { get; set; }
    public string? RegexPattern { get; set; }
}