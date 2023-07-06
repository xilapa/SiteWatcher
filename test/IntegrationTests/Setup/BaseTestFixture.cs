using System.Data.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.IntegrationTests.Setup;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Setup;

public class BaseTestFixture : IAsyncLifetime
{
    public CustomWebApplicationFactory<Program> AppFactory = null!;
    public HttpClient Client = null!;

    private readonly BaseTestFixtureOptionsBuilder _optionsBuilder;
    private PostgreSqlContainer? _postgresContainer;
    private DbConnection? _dbConnection;

    public BaseTestFixture()
    {
        _optionsBuilder = new BaseTestFixtureOptionsBuilder();
    }

    protected virtual void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    { }

    public virtual async Task InitializeAsync()
    {
        OnConfiguringTestServer(_optionsBuilder);
        var opts = _optionsBuilder.Build();
        var (connectionString, dbConnection) = await GetDatabaseConnection(opts);

        AppFactory = new CustomWebApplicationFactory<Program>(opts, connectionString, dbConnection);
        Client = AppFactory.CreateClient(new WebApplicationFactoryClientOptions {AllowAutoRedirect = false});

        await using var context = AppFactory.GetContext();
        await context.Database.EnsureCreatedAsync();
        await Database.SeedDatabase(context, AppFactory.CurrentTime, opts.DatabaseType);
    }

    private async Task<(string, DbConnection?)> GetDatabaseConnection(BaseTestFixtureOptions opts)
    {
        string connectionString;
        switch (opts.DatabaseType)
        {
            case DatabaseType.SqliteInMemory:
                connectionString = "DataSource=:memory:";
                _dbConnection = new SqliteConnection(connectionString);
                // handling the connection manually to avoid DbContext close the connection 
                // and database being destroyed
                await _dbConnection.OpenAsync();
                break;

            case DatabaseType.SqliteOnDisk:
                connectionString = $"DataSource={Guid.NewGuid()}.db";
                _dbConnection = new SqliteConnection(connectionString);
                break;

            case DatabaseType.Postgres:
                var testSettings = new ConfigurationBuilder()
                    .AddJsonFile("testsettings.json")
                    .Build()
                    .Get<TestSettings>();
                EnvironmentUtils.ApplyEnvironmentVariables(testSettings!);

                _postgresContainer = new PostgreSqlBuilder()
                    .WithDatabase($"testDb{Guid.NewGuid()}")
                    .Build();
                await _postgresContainer.StartAsync();
                connectionString = _postgresContainer.GetConnectionString();
                break;

            default:
                throw new ArgumentException(nameof(opts.DatabaseType));
        }
        return (connectionString, _dbConnection);
    }

    public async Task DisposeAsync()
    {
        Client.CancelPendingRequests();
        Client.Dispose();

        await using var context = AppFactory.GetContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.CloseConnectionAsync();
        if (_dbConnection is not null) await _dbConnection.DisposeAsync();
        if (_postgresContainer is not null) await _postgresContainer.DisposeAsync();

        // Connections are pooled by SQLite in order to improve performance.
        // It means when you call Close method on a connection object,
        // connection to database may still be alive (in the background)
        // so that next Open method become faster.
        // When you known that you don't want a new connection anymore,
        // calling ClearAllPools closes all the connections
        // which are alive in the background and file handle(s?)
        // to the db file get released.Then db file may get removed,
        // deleted or used by another process.
        // https://stackoverflow.com/questions/8511901/system-data-sqlite-close-not-releasing-database-file
        SqliteConnection.ClearAllPools();
        NpgsqlConnection.ClearAllPools();

        await AppFactory.DisposeAsync();
    }
}