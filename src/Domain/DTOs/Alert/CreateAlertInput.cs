using SiteWatcher.Domain.Enums;

namespace Domain.DTOs.Alert;

public class CreateAlertInput
{
    public string Name { get; set; }
    public EFrequency Frequency { get; set; }
    public string SiteName { get; set; }
    public string SiteUri { get; set; }
    public EWatchMode WatchMode { get; set; }

    // Term watch
    public string? Term { get; set; }
}