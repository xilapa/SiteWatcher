using System.Data.Common;
using Microsoft.Data.Sqlite;
using Npgsql;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.DapperRepositories;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class TestDapperContext : DapperContext
{
    private readonly string _connectionString;
    private readonly DatabaseType _databaseType;

    public TestDapperContext(IAppSettings appSettings, string connectionString, DatabaseType databaseType) : base(appSettings)
    {
        _connectionString = connectionString;
        _databaseType = databaseType;
    }

    protected override DbConnection CreateConnection(string connectionString)
    {
        if (DatabaseType.PostgresOnDocker.Equals(_databaseType))
            return new NpgsqlConnection(_connectionString);

        return new SqliteConnection(_connectionString);
    }
}