using Microsoft.Extensions.Logging;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.DomainServices;
using SiteWatcher.Domain.Users.Repositories;

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
    private readonly IUserRepository _userRepository;
    private readonly IUserAlertsService _userAlertsService;
    private readonly ISession _session;
    private readonly ICache _cache;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ExecuteAlertsCommandHandler> _logger;

    public ExecuteAlertsCommandHandler(IUserRepository userRepository, IUserAlertsService userAlertsService,
            ISession session, ICache cache, IUnitOfWork uow, ILogger<ExecuteAlertsCommandHandler> logger)
    {
        _userRepository = userRepository;
        _userAlertsService = userAlertsService;
        _session = session;
        _cache = cache;
        _uow = uow;
        _logger = logger;
    }

    public async Task Handle(ExecuteAlertsCommand cmmd, CancellationToken ct)
    {
        if (cmmd.Frequencies.Count == 0)
        {
            _logger.LogInformation("{Date} - Execute Alerts: No Frequencies to execute", DateTime.UtcNow);
            return;
        }

        try
        {
            _logger.LogInformation("{Date} - Execute Alerts Started: {Frequencies}", DateTime.UtcNow,
                cmmd.Frequencies);
            await ExecuteAlertsLoop(cmmd.Frequencies, ct);
            await _cache.DeleteKeysWith(CacheKeys.AlertsKeyPrefix);
            _logger.LogInformation("{Date} - Execute Alerts Finished: {Frequencies}", DateTime.UtcNow,
                cmmd.Frequencies);
        }
        catch(Exception e)
        {
            _logger.LogError(e, "{Date} - Execute Alerts Can't Finish: {Frequencies}, Exception: {msg}",
                DateTime.UtcNow, cmmd.Frequencies, e.Message);
        }
    }

    private async Task ExecuteAlertsLoop(List<Frequencies> freqs, CancellationToken ct)
    {
        var loop = true;
        DateTime? lastCreatedDate = null;
        while(loop)
        {
            var usersWithAlerts = (await _userRepository
                .GetUserWithPendingAlertsAsync(freqs, 50, lastCreatedDate, ct))
                .ToArray();

            if (usersWithAlerts.Length == 0)
            {
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
                    await _uow.SaveChangesAsync(ct);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{Date} - Execute Alerts Error Saving Changes - User: {UserId}",
                        DateTime.UtcNow, user.Id);
                }
            }
        }
    }

    private void LogAlertExecutionErrors(List<AlertExecutionError> errors)
    {
        foreach (var error in errors)
        {
            _logger.LogError(error.Exception, "{Date} - Execute Alerts Error - AlertId: {AlertId} - {Msg}",
                DateTime.UtcNow, error.AlertId, error.Exception.Message);
        }
    }
}