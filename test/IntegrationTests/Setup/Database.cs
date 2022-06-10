using SiteWatcher.Infra;
using SiteWatcher.IntegrationTests.Utils;

namespace SiteWatcher.IntegrationTests.Setup;

public static class Database
{
    public static async Task SeedDatabase(SiteWatcherContext ctx, DateTime currentTime)
    {
        await SeedUsers(ctx, currentTime);
    }

    private static async Task SeedUsers(SiteWatcherContext ctx, DateTime currentTime)
    {
        var usersVm = new []
        {
            Users.Xilapa, Users.Xulipa
        };
        foreach (var userVm in usersVm)
            ctx.Add(userVm.ToModel(currentTime));

        await ctx.SaveChangesAsync();
    }
}