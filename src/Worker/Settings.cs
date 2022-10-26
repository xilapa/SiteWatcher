using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Worker;

public class WorkerSettings
{
    public WorkerSettings()
    {
        Triggers = new Dictionary<EFrequency, string>();
    }

    public bool EnableJobs { get; set; }

    public Dictionary<EFrequency, string> Triggers { get; set; }
}