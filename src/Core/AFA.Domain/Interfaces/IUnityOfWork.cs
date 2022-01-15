using System.Threading;
using System.Threading.Tasks;

namespace AFA.Domain.Interfaces;

public interface IUnityOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}