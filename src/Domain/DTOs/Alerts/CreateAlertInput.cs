using SiteWatcher.Domain.Enums;

namespace Domain.DTOs.Alerts;

public class CreateAlertInput
{
    public CreateAlertInput(string name, EFrequency frequency, string siteName,
     string siteUri, EWatchMode watchMode, string? term, bool? notifyOnDisappearance,
     string? regexPattern)
    {
        Name = name;
        Frequency = frequency;
        SiteName = siteName;
        SiteUri = siteUri;
        WatchMode = watchMode;
        Term = term;
        NotifyOnDisappearance = notifyOnDisappearance;
        RegexPattern = regexPattern;
    }

    public CreateAlertInput()
    { }

    public string Name { get; set; } = null!;
    public EFrequency Frequency { get; set; }
    public string SiteName { get; set; } = null!;
    public string SiteUri { get; set; } = null!;
    public EWatchMode WatchMode { get; set; }

    // Term watch
    public string? Term { get; set; }

    // Regex watch
    public bool? NotifyOnDisappearance { get; set; }
    public string? RegexPattern { get; set; }
}