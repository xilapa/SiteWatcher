using System.Text.Json;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Http;
using SiteWatcher.Worker.Messaging;
using SiteWatcher.Worker.Persistence;
using static SiteWatcher.Infra.Http.HttpRetryPolicies;

namespace SiteWatcher.Worker.Consumers;

public sealed class WatchAlertsConsumer : IWatchAlertsConsumer, ICapSubscribe
{
    private readonly ILogger<WatchAlertsConsumer> _logger;
    private readonly SiteWatcherContext _context;
    private readonly ICapPublisher _capPublisher;
    private readonly WorkerSettings _settings;
    private readonly HttpClient _httpClient;

    public WatchAlertsConsumer(ILogger<WatchAlertsConsumer> logger, SiteWatcherContext context, ICapPublisher capPublisher,
        IOptions<WorkerSettings> settings, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _context = context;
        _capPublisher = capPublisher;
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient();
    }

    // CAP uses this attribute to create a queue and bind it with a routing key.
    // The message name is the routing key and group name is used to create the queue name.
    // Cap append the version on the queue name (e.g., queue-name.v1)
    [CapSubscribe(RoutingKeys.WatchAlerts, Group = RoutingKeys.WatchAlerts)]
    public async Task Consume(WatchAlertsMessage message, [FromCap] CapHeader capHeader, CancellationToken cancellationToken)
    {
        var messageId = capHeader[MessageHeaders.MessageIdKey]!;
        var hasBeenProcessed = await _context
            .HasBeenProcessed(messageId, nameof(WatchAlertsConsumer));

        if (hasBeenProcessed)
        {
            _logger.LogInformation("{Date} Message has already been processed: {MessageId}", DateTime.UtcNow, capHeader[MessageHeaders.MessageIdKey]!);
            return;
        }

        using var transaction = _context.Database.BeginTransaction(_capPublisher, autoCommit: false);

        await GenerateNotifications(message, cancellationToken);

        await _context.SaveChangesAsync(CancellationToken.None);
        await _context.MarkMessageAsConsumed(messageId, nameof(WatchAlertsConsumer));

        await transaction.CommitAsync(CancellationToken.None);

        var messageJson = JsonSerializer.Serialize(message);
        _logger.LogInformation("{Date} Message consumed: {Message}", DateTime.UtcNow, messageJson);
    }

    private async Task GenerateNotifications(WatchAlertsMessage message, CancellationToken cancellationToken)
    {
        var frequencies = message.Frequencies;

        var usersWithAlerts = _context.Users
            .Include(u => u.Alerts.Where(_ => frequencies.Contains(_.Frequency)))
            .ThenInclude(a => a.WatchMode)
            .AsAsyncEnumerable();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = _settings.Consumers.ConcurrencyBetweenMessages
        };

        await Parallel.ForEachAsync(usersWithAlerts, parallelOptions, GenerateNotification);
    }

    private async ValueTask GenerateNotification(User user, CancellationToken cancellationToken)
    {
        var alertsToNotifySuccess = new List<AlertToNotify>();
        var alertsToNotifyError = new List<AlertToNotify>();

        foreach (var alert in user.Alerts)
        {
            // HttpClient is thread safe
            // https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-6.0#thread-safety
            var (htmlStream, sucess) = await _httpClient
                .GetStreamAsyncWithRetries(alert.Site.Uri,
                                            _logger,
                                            AnyErrorRetryWithTimeout,
                                            cancellationToken);

            if (!sucess)
            {
                alertsToNotifyError.Add(new AlertToNotify(alert, user.Language));
                _logger.LogError("{CurrentTime}: Error on fetching site : {Site} from User {UserId} - Alert {AlertId}",
                     DateTime.Now, alert.Site.Uri, user.Id, alert.Id);
                continue;
            }

            // TODO: create a wrapper for date, to make tests possible
            var alertToNotify = await alert.VerifySiteHtml(htmlStream, DateTime.UtcNow);
            if (alertToNotify != null)
                alertsToNotifySuccess.Add(alertToNotify);
        }

        if (alertsToNotifySuccess.Count == 0 && alertsToNotifyError.Count == 0)
            return;

        // Get the related notification Ids
        var notificationIds = user.Alerts
            .SelectMany(a => a.Notifications)
            .Select(n => n.Id);

        // TODO: publish notification message
    }
}

public interface IWatchAlertsConsumer
{
    Task Consume(WatchAlertsMessage message, CapHeader capHeader, CancellationToken cancellationToken);
}