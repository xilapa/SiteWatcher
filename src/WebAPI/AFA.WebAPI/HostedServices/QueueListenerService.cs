using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AFA.WebAPI.HostedServices;

public class QueueListenerService : BackgroundService
{
    private readonly ILogger<QueueListenerService> logger;
    private readonly IBackgroundTaskQueue backgroundTaskQueue;
    private readonly IServiceProvider serviceScopeFactory;

    public QueueListenerService(ILogger<QueueListenerService> logger, IBackgroundTaskQueue backgroundTaskQueue, IServiceProvider  serviceScopeFactory)
    {
        this.logger = logger;
        this.backgroundTaskQueue = backgroundTaskQueue;
        this.serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Queue listerner is running");

        while(!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
            var (func, tipo) = await  backgroundTaskQueue.DequeueAsync(stoppingToken);
            logger.LogInformation("Queue listerner tried to DequeueAsync an background work item");

            using(var scope = serviceScopeFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService(tipo);
                await func(stoppingToken, service);
            }

            logger.LogInformation("Queue listerner fineshed the background work item");

        }
    }
}