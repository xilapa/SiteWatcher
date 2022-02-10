using System;
using System.Threading;
using System.Threading.Tasks;

namespace AFA.Domain.Interfaces;

public interface IUnityOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}