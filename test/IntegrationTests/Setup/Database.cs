using Microsoft.EntityFrameworkCore;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.IntegrationTests.Utils;

namespace SiteWatcher.IntegrationTests.Setup;

public static class Database
{
    public static async Task SeedDatabase(SiteWatcherContext ctx, DateTime currentTime, DatabaseType databaseType)
    {
        await SeedUsers(ctx, currentTime);
        await ApplyManualMigrations(ctx, databaseType);
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

    private static async ValueTask ApplyManualMigrations(SiteWatcherContext ctx, DatabaseType databaseType)
    {
        if(!DatabaseType.Postgres.Equals(databaseType))
            return;

        await ctx.Database.ExecuteSqlRawAsync(ManualMigrations.AlertSearchTrigramIndex);
    }
}