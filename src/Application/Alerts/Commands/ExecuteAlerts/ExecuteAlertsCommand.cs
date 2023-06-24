using SiteWatcher.Application.Common.Commands;
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
    public ExecuteAlertsCommand(IEnumerable<Frequencies> frequencies)
    {
        Frequencies = frequencies;
    }

    public IEnumerable<Frequencies> Frequencies { get; }
}

public sealed class ExecuteAlertsCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAlertsService _userAlertsService;
    private readonly ISession _session;
    private readonly ICache _cache;
    private readonly IUnitOfWork _uow;

    public ExecuteAlertsCommandHandler(IUserRepository userRepository, IUserAlertsService userAlertsService,
            ISession session, ICache cache, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _userAlertsService = userAlertsService;
        _session = session;
        _cache = cache;
        _uow = uow;
    }

    public async Task<CommandResult> Handle(ExecuteAlertsCommand cmmd, CancellationToken ct)
    {
        if (Enumerable.Empty<Frequencies>().Equals(cmmd.Frequencies))
            return CommandResult.Empty();

        try
        {
            await ExecuteAlertsLoop(cmmd.Frequencies, ct);
            await _cache.DeleteKeysWith(CacheKeys.AlertsKeyPrefix);
        }
        catch
        {
            return CommandResult.FromValue(false);
        }

        return CommandResult.FromValue(true);
    }

    private async Task ExecuteAlertsLoop(IEnumerable<Frequencies> freqs, CancellationToken ct)
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
                await _userAlertsService.ExecuteAlerts(user,_session.Now, ct);
                await _uow.SaveChangesAsync(ct);
            }
        }
    }
}