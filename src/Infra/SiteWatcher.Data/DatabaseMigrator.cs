using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SiteWatcher.Data;

public class DatabaseMigrator
{
    private readonly SiteWatcherContext _context;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(SiteWatcherContext context, ILogger<DatabaseMigrator> logger)
    {
        this._context = context;
        this._logger = logger;
    }

    public async Task<string> Migrate()
    {
        if(!(await _context.Database.GetPendingMigrationsAsync()).Any())
            return "No pending migrations";

        var stopwatch = new Stopwatch();
        _logger.LogInformation("Database migration started at {Date}", DateTime.UtcNow);

        stopwatch.Start();
        await _context.Database.MigrateAsync();
        stopwatch.Stop();

        _logger.LogInformation("Database migration finished at {Date} with a duration of {Duration} ms", DateTime.UtcNow, stopwatch.ElapsedMilliseconds);

        return $"Database migration finished with a duration of {stopwatch.ElapsedMilliseconds} s";
    }
}