namespace Domain.DTOs.Alerts;

public class AlertDetailsDto
{
    public int Id { get; set; }
    public string SiteUri { get; set; } = null!;
    public int WatchModeId { get; set; }
    public string? Term { get; set; }
}