using System.Data;

namespace SiteWatcher.Application.Interfaces;

public interface IDapperRepository<T>
{
    Task<T> UsingConnectionAsync(Func<IDbConnection, Task<T>> func);
}