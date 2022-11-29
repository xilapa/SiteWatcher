using System.Collections.Concurrent;
using System.Text.Json;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.ValueObjects;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Http;
using SiteWatcher.Worker.Messaging;
using SiteWatcher.Worker.Utils;
using static SiteWatcher.Infra.Http.HttpRetryPolicies;

namespace SiteWatcher.Worker.Jobs;

public sealed class WatchAlertsJob : IJob
{
    private readonly ILogger<WatchAlertsJob> _logger;
    private readonly SiteWatcherContext _context;
    private readonly ICapPublisher _capPublisher;
    private readonly WorkerSettings _settings;
    private readonly HttpClient _httpClient;

    public static string Name => nameof(WatchAlertsJob);

    public WatchAlertsJob(ILogger<WatchAlertsJob> logger, SiteWatcherContext context, ICapPublisher capPublisher,
        IOptions<WorkerSettings> settings, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _context = context;
        _capPublisher = capPublisher;
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var frequencies = GetAlertFrequenciesToWatch();
        var message = new WatchAlertsMessage(frequencies);

        _logger.LogInformation("{Date} - Watch Alerts Started: {Frequencies}", DateTime.UtcNow, frequencies);

        using var transaction = _context.Database.BeginTransaction(_capPublisher, autoCommit: false);

        await GenerateNotifications(message, context.CancellationToken);

        await _context.SaveChangesAsync(CancellationToken.None);

        await transaction.CommitAsync(CancellationToken.None);

        var messageJson = JsonSerializer.Serialize(message);
        _logger.LogInformation("{Date} Message consumed: {Message}", DateTime.UtcNow, messageJson);
    }

    private static IEnumerable<Frequencies> GetAlertFrequenciesToWatch()
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

    // TODO: generate notification and generate notifications should be an use case of the application
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
            MaxDegreeOfParallelism = Environment.ProcessorCount - 2 > 0 ? Environment.ProcessorCount - 2 : 1
        };

        var messagesToPublish =
            new ConcurrentBag<(EmailNotificationMessage, Dictionary<string, string>)>();
        await Parallel.ForEachAsync(usersWithAlerts, parallelOptions,
        (user, ct) => GenerateNotification(user, messagesToPublish, ct));

        foreach (var (msg, headers) in messagesToPublish)
            await _capPublisher.PublishAsync(RoutingKeys.EmailNotification, msg, headers!, cancellationToken);
    }

    private async ValueTask GenerateNotification(User user, ConcurrentBag<(EmailNotificationMessage, Dictionary<string, string>)> messagesToPublishBag, CancellationToken cancellationToken)
    {
        var alertsToNotifySuccess = new List<AlertToNotify>();
        var alertsToNotifyError = new List<AlertToNotify>();

        // TODO: move this to a domain service
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
                alertsToNotifyError.Add(alert.GenerateAlertToNotify(DateTime.UtcNow));
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
        // end of domain service

        // TODO: move the generation of email notification message to a domain service
        var (email, emailNotificationMessage) = await EmailNotificationMessageFactory
            .Generate(user, alertsToNotifySuccess, alertsToNotifyError, _settings.SiteWatcherUri);

        // Save the email message that will be published to be sent
        // The notification sender will set the DateSent value
        _context.Add(email);

        var alertToNotifySuccessIds = alertsToNotifySuccess.Select(_ => _.NotificationId);
        var alertToNotifyErrorsIds = alertsToNotifyError.Select(_ => _.NotificationId);
        bool _;
        foreach(var alert in user.Alerts)
            _ = alert.SetEmail(email, alertToNotifySuccessIds) || alert.SetEmail(email, alertToNotifyErrorsIds);

        // end of domain service

        // Publish the email message on the bus
        // Use the email id as the message id
        var headers = new Dictionary<string, string>
        {
            [MessageHeaders.MessageIdKey] = email.Id.ToString()
        };

        // Message is published after, because the database connection is in use by the async enumerable
        messagesToPublishBag.Add((emailNotificationMessage, headers));
    }
}