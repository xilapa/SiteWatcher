using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Application.IdempotentConsumers;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Worker.Jobs;

public sealed class CleanIdempotentConsumersPeriodically : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PeriodicTimer _timer;

    public CleanIdempotentConsumersPeriodically(IServiceScopeFactory scopeProvider, IAppSettings settings)
    {
        _scopeFactory = scopeProvider;
        _timer = new PeriodicTimer(settings.IsDevelopment ? TimeSpan.FromSeconds(15) : TimeSpan.FromDays(1));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        CleanIdempotentConsumers(stoppingToken);

    private async Task CleanIdempotentConsumers(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<CleanIdempotentConsumers>();

            await handler.Clean(cancellationToken);

            await _timer.WaitForNextTickAsync(cancellationToken);
        }
    }
}