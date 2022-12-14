using System.Threading.Channels;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.DomainServices;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Repositories;

namespace SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;

public sealed class ExecuteAlertsCommand
{
    public ExecuteAlertsCommand()
    {
        Frequencies = Enumerable.Empty<Frequencies>();
    }

    public ExecuteAlertsCommand(IEnumerable<Frequencies> frequencies)
    {
        Frequencies = frequencies;
    }

    public IEnumerable<Frequencies> Frequencies { get; set; }
}

public sealed class ExecuteAlertsCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpClient _httpClient;
    private readonly IUserAlertsService _userAlertsService;
    private readonly ISession _session;
    private readonly IAppSettings _appSettings;

    public ExecuteAlertsCommandHandler(IUserRepository userRepository, IHttpClient httpClient, IUserAlertsService userAlertsService, ISession session, IAppSettings appSettings)
    {
        _userRepository = userRepository;
        _httpClient = httpClient;
        _userAlertsService = userAlertsService;
        _session = session;
        _appSettings = appSettings;
    }

    public async Task<CommandResult> Handle(ExecuteAlertsCommand cmmd, CancellationToken ct)
    {
        if (Enumerable.Empty<Frequencies>().Equals(cmmd.Frequencies))
            return CommandResult.Empty();

        var usersWithAlerts = _userRepository.GetUserWithAlertsAsync(cmmd.Frequencies, ct);

        // get the user site streams in parallel and send to a channel to generate one notification at a time
        var streamsChan = Channel
            .CreateBounded<(User, Dictionary<AlertId, Stream?>)>(
                new BoundedChannelOptions(1)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true
                });

        // channel to send the notifications back
        var notificationsChan = Channel
            .CreateBounded<List<NotificationToSend>>(
                new BoundedChannelOptions(1)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = true
                });

        // run a task to consume the items from the streamsChan one by one
        _ = Task.Run(async () =>
        {
            var notifications = new List<NotificationToSend>();
            await foreach (var userAlerts in streamsChan.Reader.ReadAllAsync(ct))
            {
                var notif = await _userAlertsService
                    .ExecuteAlerts(userAlerts.Item1, userAlerts.Item2, _session.Now, _appSettings.FrontEndUrl);
                if(notif != null)
                    notifications.Add(notif);
            }
            await notificationsChan.Writer.WriteAsync(notifications);
            notificationsChan.Writer.Complete();
        })
        .ContinueWith(task =>
            {
                if (task.Exception is not null)
                    throw task.Exception;
            },ct);

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = ct,
            MaxDegreeOfParallelism = 2
        };
        await Parallel.ForEachAsync(
            usersWithAlerts,
            parallelOptions,
            (user, ct) => GetUserSiteStreams(user, streamsChan.Writer, ct)
        );

        streamsChan.Writer.Complete();

        // read the notifications from the background task
        var notifications = await notificationsChan.Reader.ReadAsync(ct);

        return CommandResult.FromValue(notifications);
    }

    private async ValueTask GetUserSiteStreams(User user, ChannelWriter<(User, Dictionary<AlertId, Stream?>)> chan, CancellationToken ct)
    {
        var streamsDict = new Dictionary<AlertId, Stream?>(user.Alerts.Count);
        foreach (var alert in user.Alerts)
        {
            // HttpClient is thread safe
            // https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-6.0#thread-safety
            var htmlStream = await _httpClient.GetStreamAsync(alert.Site.Uri, ct);
            streamsDict.Add(alert.Id, htmlStream);
        }
        await chan.WriteAsync((user, streamsDict), ct);
    }
}