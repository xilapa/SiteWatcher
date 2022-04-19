namespace SiteWatcher.Application.Interfaces;

public interface IUnityOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}