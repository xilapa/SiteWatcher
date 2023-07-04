using DotNetCore.CAP;
using Mediator;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra;
using IPublisher = SiteWatcher.Domain.Common.Services.IPublisher;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class PostgresTestContext : SiteWatcherContext
{
    private readonly string _connectionString;

    public PostgresTestContext(IAppSettings appSettings, IMediator mediator, string connectionString, ICapPublisher cap,
        IPublisher publisher) :
        base(appSettings, mediator, cap, publisher)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }
}