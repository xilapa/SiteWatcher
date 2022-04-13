

namespace SiteWatcher.Domain.Interfaces;

public interface IUnityOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}