using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Infra;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class SqliteContext : SiteWatcherContext
{
    private readonly SqliteConnection _connection;

    public SqliteContext(IAppSettings appSettings, IMediator mediator, SqliteConnection connection) :
        base(appSettings, mediator)
    {
        _connection = connection;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine);
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
}