using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiteWatcher.Domain.Interfaces;

public interface IUnityOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}