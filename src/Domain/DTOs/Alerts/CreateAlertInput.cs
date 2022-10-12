using SiteWatcher.Domain.Enums;

namespace Domain.DTOs.Alerts;

public class CreateAlertInput
{
    public CreateAlertInput(string name, EFrequency frequency, string siteName, string siteUri, EWatchMode watchMode, string? term)
    {
        Name = name;
        Frequency = frequency;
        SiteName = siteName;
        SiteUri = siteUri;
        WatchMode = watchMode;
        Term = term;
    }

    public CreateAlertInput()
    { }

    public string Name { get; set; }
    public EFrequency Frequency { get; set; }
    public string SiteName { get; set; }
    public string SiteUri { get; set; }
    public EWatchMode WatchMode { get; set; }

    // Term watch
    public string? Term { get; set; }
}