using System.Data;

namespace SiteWatcher.Application.Interfaces;

public interface IDapperRepository<T>
{
    Task<T> UsingConnectionAsync(Func<IDbConnection, Task<T>> func);
    Task UsingConnectionAsync(Func<IDbConnection, Task> func);
}