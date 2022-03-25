using System.Data;
using Npgsql;
using SiteWatcher.Domain.Interfaces;

namespace SiteWatcher.Data.DapperRepositories;

public abstract class DapperRepository<T> : IDapperRepository<T>
{
    private readonly string connectionString;

    public DapperRepository(string connectionString) => this.connectionString = connectionString;

    public async Task<T> UsingConnectionAsync(Func<IDbConnection, Task<T>> func)
    {
        using(var connection = new NpgsqlConnection(connectionString))
        {
            if(connection.State == ConnectionState.Closed)
                await connection.OpenAsync();

            return await func(connection);
        }
    }
}