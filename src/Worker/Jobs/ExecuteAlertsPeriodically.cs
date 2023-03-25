using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Worker.Jobs;

public sealed class ExecuteAlertsPeriodically : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExecuteAlertsPeriodically> _logger;
    private readonly PeriodicTimer _timer;

    public ExecuteAlertsPeriodically(IServiceScopeFactory scopeProvider, ILogger<ExecuteAlertsPeriodically> logger, IAppSettings settings)
    {
        _scopeFactory = scopeProvider;
        _logger = logger;
        _timer = new PeriodicTimer(settings.IsDevelopment ? TimeSpan.FromMinutes(1) : TimeSpan.FromHours(1));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
         Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<ExecuteAlertsCommandHandler>();
                await ExecuteAlerts(handler, stoppingToken);
                await _timer.WaitForNextTickAsync(stoppingToken);
            }
        }, stoppingToken);

    private async Task ExecuteAlerts(ExecuteAlertsCommandHandler handler, CancellationToken ct)
    {
        var frequencies = GetAlertFrequenciesForCurrentHour();

        _logger.LogInformation("{Date} - Execute Alerts Started: {Frequencies}", DateTime.UtcNow, frequencies);

        var ok = await handler.Handle(new ExecuteAlertsCommand(frequencies), ct);

        _logger.LogInformation("{Date} - Execute Alerts Finished: {Frequencies} - Success: {success}",
            DateTime.UtcNow, frequencies, (ok as ValueResult<bool>)!.Value);
    }

    private static IEnumerable<Frequencies> GetAlertFrequenciesForCurrentHour()
    {
        var alertFrequenciesToWatch = new List<Frequencies>();

        // TODO: Wrapper for time, to make tests possible
        var currentHour = DateTime.Now.Hour;

        // If the rest of current hour/ frequency is zero, this alerts of this frequency needs to be watched
        foreach (var frequency in Enum.GetValues<Frequencies>())
        {
            if (currentHour % (int)frequency == 0)
                alertFrequenciesToWatch.Add(frequency);
        }

        return alertFrequenciesToWatch;
    }
}