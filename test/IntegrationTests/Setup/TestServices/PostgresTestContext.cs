using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class PostgresTestContext : SiteWatcherContext
{
    private readonly string _connectionString;

    public PostgresTestContext(IAppSettings appSettings, IMediator mediator, string connectionString) :
        base(appSettings, mediator)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }
}