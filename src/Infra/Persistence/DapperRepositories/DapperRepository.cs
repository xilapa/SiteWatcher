using System.Data;
using Npgsql;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Infra.DapperRepositories;

public abstract class DapperRepository<T> : IDapperRepository<T>
{
    private readonly string _connectionString;

    protected DapperRepository(IAppSettings appSettings) =>
        _connectionString = appSettings.ConnectionString;

    public async Task<T> UsingConnectionAsync(Func<IDbConnection, Task<T>> func)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        if(connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

        return await func(connection);
    }

    public async Task UsingConnectionAsync(Func<IDbConnection, Task> func)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        if(connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

        await func(connection);
    }
}