using System.Data.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class PostgresTestContext : SiteWatcherContext
{
    private readonly DbConnection _connection;

    public PostgresTestContext(IAppSettings appSettings, IMediator mediator, DbConnection connection) :
        base(appSettings, mediator)
    {
        _connection = connection;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connection);
    }
}