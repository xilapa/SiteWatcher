using System;
using System.Data;
using System.Threading.Tasks;

namespace SiteWatcher.Domain.Interfaces;

public interface IDapperRepository<T>
{
    Task<T> UsingConnectionAsync(Func<IDbConnection, Task<T>> func);
}