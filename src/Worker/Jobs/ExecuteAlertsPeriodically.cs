using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Cms;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.DomainServices;

namespace SiteWatcher.Worker.Jobs;

public sealed class ExecuteAlertsPeriodically : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExecuteAlertsPeriodically> _logger;
    private readonly PeriodicTimer _timer;

    public ExecuteAlertsPeriodically(IServiceScopeFactory scopeProvider, IAppSettings settings,
        ILogger<ExecuteAlertsPeriodically> logger)
    {
        _scopeFactory = scopeProvider;
        _logger = logger;
        _timer = new PeriodicTimer(settings.IsDevelopment ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(10));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        ExecuteAlerts(stoppingToken);

    private async Task ExecuteAlerts(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<ExecuteAlertsCommandHandler>();
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                var frequencies = AlertFrequencies.GetCurrentFrequencies(session.Now);
                await handler.Handle(new ExecuteAlertsCommand(frequencies), cancellationToken);

                await _timer.WaitForNextTickAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error during alerts execution");
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }
    }
}