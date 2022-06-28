﻿namespace SiteWatcher.Domain.Models.Alerts.WatchModes;

public class AnyChangesWatch : WatchMode
{
    // ctor for EF
    protected AnyChangesWatch() : base()
    { }

    public AnyChangesWatch(DateTime currentDate) : base(currentDate)
    { }

    public string? HtmlText { get; private set; }
}