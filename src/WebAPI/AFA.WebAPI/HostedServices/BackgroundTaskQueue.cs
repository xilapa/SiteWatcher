using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AFA.WebAPI.HostedServices;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<(Func<CancellationToken,dynamic, Task> func, Type tipo)> queue;

    public BackgroundTaskQueue()
    {
        var capacity = Environment.ProcessorCount;
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        queue = Channel.CreateBounded<(Func<CancellationToken,dynamic, Task> func, Type tipo)>(options);
    }

    public async Task<(Func<CancellationToken,dynamic,Task> func, Type tipo)> DequeueAsync(CancellationToken stoppingToken)
    {
        return await queue.Reader.ReadAsync(stoppingToken);
    }


    public async Task QueueBackgroundWorkItemAsync((Func<CancellationToken,dynamic,Task> func, Type tipo) workItem)
    {
        await queue.Writer.WriteAsync(workItem);
    }
}