using System.Data;
using Npgsql;
using SiteWatcher.Domain.Interfaces;

namespace SiteWatcher.Data.DapperRepositories;

public abstract class DapperRepository<T> : IDapperRepository<T>
{
    private readonly string _connectionString;

    public DapperRepository(string connectionString) =>
        _connectionString = connectionString;

    public async Task<T> UsingConnectionAsync(Func<IDbConnection, Task<T>> func)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        if(connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

        return await func(connection);
    }
}