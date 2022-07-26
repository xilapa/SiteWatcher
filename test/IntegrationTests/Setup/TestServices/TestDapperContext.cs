using System.Data.Common;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.DapperRepositories;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class TestDapperContext : DapperContext
{
    private readonly DbConnection _sqliteConnection;

    public TestDapperContext(IAppSettings appSettings, DbConnection sqliteConnection) : base(appSettings)
    {
        _sqliteConnection = sqliteConnection;
    }

    protected override DbConnection CreateConnection(string connectionString) =>
        _sqliteConnection;
}