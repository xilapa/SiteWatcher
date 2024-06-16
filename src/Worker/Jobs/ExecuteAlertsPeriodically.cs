using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.DomainServices;

namespace SiteWatcher.Worker.Jobs;

public sealed partial class ExecuteAlertsPeriodically : BackgroundService
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
        ExecuteAlertsLoop(stoppingToken);

    private async Task ExecuteAlertsLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteAlerts(cancellationToken);

                await _timer.WaitForNextTickAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                LogErrorDuringAlertsExecution(ex);
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }
    }

    private async Task ExecuteAlerts(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ExecuteAlertsCommandHandler>();
        var session = scope.ServiceProvider.GetRequiredService<ISession>();

        var frequencies = AlertFrequencies.GetCurrentFrequencies(session.Now);
        await handler.Handle(new ExecuteAlertsCommand(frequencies), cancellationToken);
    }

    [LoggerMessage(LogLevel.Error, "Error during alerts execution")]
    private partial void LogErrorDuringAlertsExecution(Exception ex);
}