using Domain.DTOs.Common;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;

namespace Domain.DTOs.Alert;

public class UpdateAlertInput
{
    public AlertId AlertId { get; set; }
    public UpdateInfo<string>? Name { get; set; }
    public UpdateInfo<EFrequency>? Frequency { get; set; }
    public UpdateInfo<string>? SiteName { get; set; }
    public UpdateInfo<string>? SiteUri { get; set; }
    public UpdateInfo<EWatchMode>? WatchMode { get; set; }
    public UpdateInfo<string>? Term { get; set; }
}