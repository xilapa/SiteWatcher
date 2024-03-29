﻿using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Domain.Alerts.DTOs;

public sealed class CreateAlertInput
{
    public CreateAlertInput(string name, Frequencies frequency, string siteName,
     string siteUri, RuleType ruleType, string? term, bool? notifyOnDisappearance,
     string? regexPattern)
    {
        Name = name;
        Frequency = frequency;
        SiteName = siteName;
        SiteUri = siteUri;
        RuleType = ruleType;
        Term = term;
        NotifyOnDisappearance = notifyOnDisappearance;
        RegexPattern = regexPattern;
    }

    public CreateAlertInput()
    { }

    public string Name { get; set; } = null!;
    public Frequencies Frequency { get; set; }
    public string SiteName { get; set; } = null!;
    public string SiteUri { get; set; } = null!;
    public RuleType RuleType { get; set; }

    // Term watch
    public string? Term { get; set; }

    // Regex watch
    public bool? NotifyOnDisappearance { get; set; }
    public string? RegexPattern { get; set; }
}