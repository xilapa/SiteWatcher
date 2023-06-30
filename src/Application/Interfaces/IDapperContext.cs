using System.Data;
using Dapper;

namespace SiteWatcher.Application.Interfaces;

public interface IDapperContext
{
    Task<T> UsingConnectionAsync<T>(Func<IDbConnection, Task<T>> func);
    Task UsingConnectionAsync(Func<IDbConnection, Task> func);
}