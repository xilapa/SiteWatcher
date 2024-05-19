using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.IdempotentConsumers;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Worker.Jobs;

public sealed class CleanIdempotentConsumersPeriodically : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanIdempotentConsumersPeriodically> _logger;
    private readonly PeriodicTimer _timer;

    public CleanIdempotentConsumersPeriodically(IServiceScopeFactory scopeProvider, IAppSettings settings,
        ILogger<CleanIdempotentConsumersPeriodically> logger)
    {
        _scopeFactory = scopeProvider;
        _logger = logger;
        _timer = new PeriodicTimer(settings.IsDevelopment ? TimeSpan.FromSeconds(15) : TimeSpan.FromDays(1));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        CleanIdempotentConsumers(stoppingToken);

    private async Task CleanIdempotentConsumers(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<CleanIdempotentConsumers>();

                await handler.Clean(cancellationToken);

                await _timer.WaitForNextTickAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on cleaning idempotent consumers table");
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }
    }
}