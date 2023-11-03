using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class SqliteContext : SiteWatcherContext
{
    private readonly DbConnection _connection;

    public SqliteContext(IAppSettings appSettings, DbConnection connection) :
        base(appSettings)
    {
        _connection = connection;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlite(_connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Change UUID to BLOB
        var uuids = modelBuilder.Model
            .GetEntityTypes().SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(Guid) || p.ClrType == typeof(UserId))
            .ToArray();

        foreach (var uuid in uuids)
            uuid.SetColumnType("BLOB");
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var inner = ex.InnerException;

            // https://www.sqlite.org/rescode.html#constraint_unique
            if ((inner as SqliteException)?.SqliteExtendedErrorCode != 2067)
                throw;

            throw GenerateUniqueViolationException(ex);
        }
    }
}