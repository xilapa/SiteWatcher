using System.Data;
using System.Data.Common;
using Npgsql;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Repositories;
using System.Threading.Tasks;
using System;

namespace SiteWatcher.Infra.DapperRepositories;

public class DapperContext : IDapperContext
{
    private readonly string _connectionString;

    public DapperContext(IAppSettings appSettings)
    {
        _connectionString = appSettings.ConnectionString;
    }

    protected virtual DbConnection CreateConnection(string connectionString) =>
        new NpgsqlConnection(connectionString);

    public async Task<T> UsingConnectionAsync<T>(Func<IDbConnection, Task<T>> func)
    {
        await using var connection = CreateConnection(_connectionString);
        if(connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

        return await func(connection);
    }

    public async Task UsingConnectionAsync(Func<IDbConnection, Task> func)
    {
        await using var connection = CreateConnection(_connectionString);
        if(connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

        await func(connection);
    }
}