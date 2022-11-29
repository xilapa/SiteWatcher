using System.Data;

namespace SiteWatcher.Common.Repositories;

public interface IDapperContext
{
    Task<T> UsingConnectionAsync<T>(Func<IDbConnection, Task<T>> func);
    Task UsingConnectionAsync(Func<IDbConnection, Task> func);
}