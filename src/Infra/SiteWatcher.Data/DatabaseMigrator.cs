using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SiteWatcher.Infra.Data;

public class DatabaseMigrator
{
    private readonly SiteWatcherContext context;
    private readonly ILogger<DatabaseMigrator> logger;

    public DatabaseMigrator(SiteWatcherContext context, ILogger<DatabaseMigrator> logger)
    {
        this.context = context;
        this.logger = logger;
    }    

    public async Task<string> Migrate()
    {
        if(!context.Database.GetPendingMigrations().Any())
            return "No pending migrations";
        
        var stopwatch = new Stopwatch();
        logger.LogInformation("Database migration started at {date}", DateTime.UtcNow);
        stopwatch.Start();
        await context.Database.MigrateAsync();
        stopwatch.Stop();
        var duration = stopwatch.ElapsedMilliseconds/1000;
        logger.LogInformation("Database migration finished at {date} with a duration of {duration} s", DateTime.UtcNow, duration);
        
        return $"Database migration finished with a duration of {duration} s";
    }
}