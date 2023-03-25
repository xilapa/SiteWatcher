using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Enums;
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

    public IEnumerable<Frequencies> Frequencies { get; set; }
}

public sealed class ExecuteAlertsCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAlertsService _userAlertsService;
    private readonly ISession _session;
    private readonly IAppSettings _appSettings;
    private readonly IPublishService _pubService;
    private readonly ICache _cache;

    public ExecuteAlertsCommandHandler(IUserRepository userRepository, IUserAlertsService userAlertsService,
            ISession session, IAppSettings appSettings, IPublishService pubService, ICache cache)
    {
        _userRepository = userRepository;
        _userAlertsService = userAlertsService;
        _session = session;
        _appSettings = appSettings;
        _pubService = pubService;
        _cache = cache;
    }

    public async Task<CommandResult> Handle(ExecuteAlertsCommand cmmd, CancellationToken ct)
    {
        if (Enumerable.Empty<Frequencies>().Equals(cmmd.Frequencies))
            return CommandResult.Empty();

        try
        {
            await _pubService.WithPublisher(pub => ExecuteAlertsLoop(cmmd.Frequencies, pub, ct), ct);
            await _cache.DeleteKeysWith(CacheKeys.AlertsKeyPrefix);
        }
        catch
        {
            return CommandResult.FromValue(false);
        }

        return CommandResult.FromValue(true);
    }

    private async Task ExecuteAlertsLoop(IEnumerable<Frequencies> freqs, IPublisher publisher, CancellationToken ct)
    {
        var loop = true;
        DateTime? lastCreatedDate = null;
        while(loop)
        {
            var usersWithAlerts = (await _userRepository
                .GetUserWithAlertsAsync(freqs, 50, lastCreatedDate, ct))
                .ToArray();

            if (usersWithAlerts.Length == 0)
            {
                loop = false;
                continue;
            }

            lastCreatedDate = usersWithAlerts[^1].CreatedAt;

            foreach(var user in usersWithAlerts)
            {
                var notif = await _userAlertsService
                    .ExecuteAlerts(user,_session.Now, _appSettings.FrontEndUrl, ct);

                if(notif == null) continue;

                // Publish the email message on the bus
                // Use the email id as the message id
                var headers = new Dictionary<string, string>
                {
                    [_appSettings.MessageIdKey] = notif.EmailNotification!.EmailId.ToString()!,
                };

                await publisher.PublishAsync(_appSettings.EmailNotificationRoutingKey, notif.EmailNotification, headers, ct);
            }
        }
    }
}