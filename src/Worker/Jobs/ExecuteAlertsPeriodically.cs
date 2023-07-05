using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.DomainServices;

namespace SiteWatcher.Worker.Jobs;

public sealed class ExecuteAlertsPeriodically : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PeriodicTimer _timer;

    public ExecuteAlertsPeriodically(IServiceScopeFactory scopeProvider, IAppSettings settings)
    {
        _scopeFactory = scopeProvider;
        _timer = new PeriodicTimer(settings.IsDevelopment ? TimeSpan.FromMinutes(1) : TimeSpan.FromHours(1));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
         Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<ExecuteAlertsCommandHandler>();
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                var frequencies = Enum.GetValues<Frequencies>().ToList();
                await handler.Handle(new ExecuteAlertsCommand(frequencies), stoppingToken);

                await _timer.WaitForNextTickAsync(stoppingToken);
            }
        }, stoppingToken);
}