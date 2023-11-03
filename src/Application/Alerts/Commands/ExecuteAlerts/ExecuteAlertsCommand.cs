using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.DomainServices;

namespace SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;

public sealed class ExecuteAlertsCommand
{
    public ExecuteAlertsCommand(List<Frequencies> frequencies)
    {
        Frequencies = frequencies;
    }

    public List<Frequencies> Frequencies { get; }
}

public sealed class ExecuteAlertsCommandHandler
{
    private readonly ISiteWatcherContext _context;
    private readonly UserAlertsService _userAlertsService;
    private readonly ISession _session;
    private readonly ICache _cache;
    private readonly IPublisher _publisher;
    private readonly ILogger<ExecuteAlertsCommandHandler> _logger;

    public ExecuteAlertsCommandHandler(ISiteWatcherContext context, UserAlertsService userAlertsService,
            ISession session, ICache cache, IPublisher publisher, ILogger<ExecuteAlertsCommandHandler> logger)
    {
        _context = context;
        _userAlertsService = userAlertsService;
        _session = session;
        _cache = cache;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(ExecuteAlertsCommand cmmd, CancellationToken ct)
    {
        if (cmmd.Frequencies.Count == 0)
        {
            _logger.LogInformation("{Date} - Execute Alerts: No Frequencies to execute", _session.Now);
            return;
        }

        try
        {
            _logger.LogInformation("{Date} - Execute Alerts Started: {Frequencies}", _session.Now,
                cmmd.Frequencies);
            await ExecuteAlertsLoop(cmmd.Frequencies, ct);
            await _cache.DeleteKeysWith(CacheKeys.AlertsKeyPrefix);
            _logger.LogInformation("{Date} - Execute Alerts Finished: {Frequencies}", _session.Now,
                cmmd.Frequencies);
        }
        catch(Exception e)
        {
            _logger.LogError(e, "{Date} - Execute Alerts Can't Finish: {Frequencies}, Exception: {msg}",
                _session.Now, cmmd.Frequencies, e.Message);
        }
    }

    private async Task ExecuteAlertsLoop(List<Frequencies> freqs, CancellationToken ct)
    {
        var loop = true;
        DateTime? lastCreatedDate = null;
        while(loop)
        {
            var usersWithAlerts = await _context
                .GetUserWithPendingAlertsAsync(lastCreatedDate, freqs, 50, _session.Now, ct);

            if (usersWithAlerts.Length == 0)
            {
                _logger.LogInformation("{Date} - Execute Alerts: No alerts to execute", _session.Now);
                loop = false;
                continue;
            }

            lastCreatedDate = usersWithAlerts[^1].CreatedAt;

            foreach(var user in usersWithAlerts)
            {
                try
                {
                    var executionStatus = await _userAlertsService.ExecuteAlerts(user,_session.Now, ct);
                    if (executionStatus.Errors.Count != 0) LogAlertExecutionErrors(executionStatus.Errors);

                    await _publisher.PublishAsync(executionStatus.AlertsTriggered, ct);

                    await _context.SaveChangesAsync(ct);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{Date} - Execute Alerts Error Saving Changes - User: {UserId}",
                        _session.Now, user.Id);
                }
            }
        }
    }

    private void LogAlertExecutionErrors(List<AlertExecutionError> errors)
    {
        foreach (var error in errors)
        {
            _logger.LogError(error.Exception, "{Date} - Execute Alerts Error - AlertId: {AlertId} - {Msg}",
                _session.Now, error.AlertId, error.Exception.Message);
        }
    }
}