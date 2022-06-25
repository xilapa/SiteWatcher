using SiteWatcher.Domain.Enums;

namespace Domain.DTOs.Alert;

public struct CreateAlertInput
{
    public string Name { get; set; }
    public EFrequency Frequency { get; set; }
    public string SiteName { get; set; }
    public string SiteUri { get; set; }
}