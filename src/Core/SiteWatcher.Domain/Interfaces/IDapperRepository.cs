using System.Data;

namespace SiteWatcher.Domain.Interfaces;

public interface IDapperRepository<T>
{
    Task<T> UsingConnectionAsync(Func<IDbConnection, Task<T>> func);
}