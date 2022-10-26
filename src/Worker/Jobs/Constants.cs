namespace SiteWatcher.Worker.Jobs;

public static class Constants
{
    public const string FireWatchForChangesKey = nameof(FireWatchForChangesJob);
    public const string WatchForChangesEveryTwoHours = nameof(FireWatchForChangesJob)+"-every-2h";
    public const string WatchForChangesEveryFourHours = nameof(FireWatchForChangesJob)+"-every-4h";
    public const string WatchForChangesEveryEightHours = nameof(FireWatchForChangesJob)+"-every-8h";
    public const string WatchForChangesEveryTwelveHours = nameof(FireWatchForChangesJob)+"-every-12h";
    public const string WatchForChangesEveryTwentyFourHours = nameof(FireWatchForChangesJob)+"-every-24h";
}