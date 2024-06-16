using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SiteWatcher.Infra;

public sealed partial class DatabaseMigrator
{
    private readonly SiteWatcherContext _context;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(SiteWatcherContext context, ILogger<DatabaseMigrator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> Migrate()
    {
        if(!(await _context.Database.GetPendingMigrationsAsync()).Any())
            return "No pending migrations";

        var stopwatch = new Stopwatch();
        LogMigrationStarted(DateTime.UtcNow);

        stopwatch.Start();
        await _context.Database.MigrateAsync();
        stopwatch.Stop();

        LogMigrationFinished(DateTime.UtcNow, stopwatch.Elapsed);

        return $"Database migration finished with a duration of {stopwatch.Elapsed}";
    }

    [LoggerMessage(LogLevel.Information, "Database migration started at {Date}")]
    private partial void LogMigrationStarted(DateTime date);

    [LoggerMessage(LogLevel.Information, "Database migration finished at {Date} with a duration of {Duration} ms")]
    private partial void LogMigrationFinished(DateTime date, TimeSpan duration);
}