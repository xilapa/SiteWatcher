namespace SiteWatcher.IntegrationTests.Utils;

public static class DateTimeUtils
{
    public static DateTime GetTimeWithHour(int hour) =>
        new (2021, 1, 1, hour, 0, 0, DateTimeKind.Utc);
}