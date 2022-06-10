using System.Data.Common;
using Microsoft.Data.Sqlite;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.DapperRepositories;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class SqliteDapperContext : DapperContext
{
    private readonly SqliteConnection _sqliteConnection;

    public SqliteDapperContext(IAppSettings appSettings, SqliteConnection sqliteConnection) : base(appSettings)
    {
        _sqliteConnection = sqliteConnection;
    }

    protected override DbConnection CreateConnection(string connectionString) =>
        _sqliteConnection;
}