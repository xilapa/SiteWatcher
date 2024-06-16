using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
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

public sealed partial class ExecuteAlertsCommandHandler
{
    private readonly ISiteWatcherContext _context;
    private readonly IUserAlertsService _userAlertsService;
    private readonly ISession _session;
    private readonly ICache _cache;
    private readonly ILogger<ExecuteAlertsCommandHandler> _logger;

    public ExecuteAlertsCommandHandler(ISiteWatcherContext context, IUserAlertsService userAlertsService,
            ISession session, ICache cache, ILogger<ExecuteAlertsCommandHandler> logger)
    {
        _context = context;
        _userAlertsService = userAlertsService;
        _session = session;
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(ExecuteAlertsCommand cmmd, CancellationToken ct)
    {
        if (cmmd.Frequencies.Count == 0)
        {
            LogNoFrequenciesToExecute(_session.Now);
            return;
        }

        try
        {
            LogExecuteAlertsStarted(_session.Now, cmmd.Frequencies);

            await ExecuteAlertsLoop(cmmd.Frequencies, ct);
            await _cache.DeleteKeysWith(CacheKeys.AlertsKeyPrefix);

            LogExecuteAlertsFinished(_session.Now, cmmd.Frequencies);
        }
        catch(Exception e)
        {
            LogExecuteAlertsCantFinish(e, _session.Now, cmmd.Frequencies, e.Message);
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
                LogNoAlertsToExecuted(_session.Now);
                loop = false;
                continue;
            }

            lastCreatedDate = usersWithAlerts[^1].CreatedAt;

            foreach(var user in usersWithAlerts)
            {
                try
                {
                    var errors = await _userAlertsService.ExecuteAlerts(user,_session.Now, ct);
                    if (errors.Count != 0) LogAlertExecutionErrors(errors);
                    await _context.SaveChangesAsync(ct);
                }
                catch (Exception e)
                {
                    LogErrorSavingChanges(e, _session.Now, user.Id);
                }
            }
        }
    }

    private void LogAlertExecutionErrors(List<AlertExecutionError> errors)
    {
        foreach (var error in errors)
            LogAlertExecutionError(_session.Now, error.AlertId, error.Exception.Message);
    }


    [LoggerMessage(LogLevel.Information, "{Date} - Execute Alerts: No Frequencies to execute")]
    private partial void LogNoFrequenciesToExecute(DateTime date);

    [LoggerMessage(LogLevel.Information, "{Date} - Execute Alerts Started: {Frequencies}")]
    private partial void LogExecuteAlertsStarted(DateTime date, List<Frequencies> frequencies);

    [LoggerMessage(LogLevel.Information, "{Date} - Execute Alerts: No alerts to execute")]
    public partial void LogNoAlertsToExecuted(DateTime date);

    [LoggerMessage(LogLevel.Information, "{Date} - Execute Alerts Finished: {Frequencies}")]
    private partial void LogExecuteAlertsFinished(DateTime date, List<Frequencies> frequencies);

    [LoggerMessage(LogLevel.Error, "{Date} - Execute Alerts Can't Finish: {Frequencies}, Exception: {Msg}")]
    private partial void LogExecuteAlertsCantFinish(Exception ex, DateTime date, List<Frequencies> frequencies, string msg);

    [LoggerMessage(LogLevel.Error, "{Date} - Execute Alerts Error - AlertId: {AlertId} - {Msg}")]
    public partial void LogAlertExecutionError(DateTime date, AlertId alertId, string msg);

    [LoggerMessage(LogLevel.Error, "{Date} - Execute Alerts Error Saving Changes - User: {UserId}")]
    public partial void LogErrorSavingChanges(Exception ex, DateTime date, UserId userId);
}