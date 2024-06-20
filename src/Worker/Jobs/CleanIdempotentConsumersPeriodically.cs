using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.IdempotentConsumers;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Worker.Jobs;

public sealed partial class CleanIdempotentConsumersPeriodically : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanIdempotentConsumersPeriodically> _logger;
    private readonly PeriodicTimer _timer;

    public CleanIdempotentConsumersPeriodically(IServiceProvider serviceProvider, IAppSettings settings,
        ILogger<CleanIdempotentConsumersPeriodically> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _timer = new PeriodicTimer(settings.IsDevelopment ? TimeSpan.FromSeconds(15) : TimeSpan.FromDays(1));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        CleanIdempotentConsumersLoop(stoppingToken);

    private async Task CleanIdempotentConsumersLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CleanIdempotentConsumers(cancellationToken);

                await _timer.WaitForNextTickAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                LogErrorOnCleaningIdempotentConsumers(ex);
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }
    }

    private async Task CleanIdempotentConsumers(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<CleanIdempotentConsumers>();

        await handler.Clean(cancellationToken);
    }

    [LoggerMessage(LogLevel.Error, "Error on cleaning idempotent consumers table")]
    private partial void LogErrorOnCleaningIdempotentConsumers(Exception ex);
}