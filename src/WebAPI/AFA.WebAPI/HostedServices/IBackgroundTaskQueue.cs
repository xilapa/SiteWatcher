using System;
using System.Threading;
using System.Threading.Tasks;

namespace AFA.WebAPI.HostedServices;

public interface IBackgroundTaskQueue 
{
    Task QueueBackgroundWorkItemAsync((Func<CancellationToken,dynamic,Task> func, Type tipo) workItem);
    Task<(Func<CancellationToken,dynamic,Task> func, Type tipo)> DequeueAsync(CancellationToken stoppingToken);
}