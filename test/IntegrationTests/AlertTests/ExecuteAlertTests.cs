using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Domain.Alerts.Entities.Triggerings;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AlertTests;

public sealed class ExecuteAlertTestsBase : BaseTestFixture
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        foreach (var freq in Enum.GetValues<Frequencies>())
        {
            var alert = await AppFactory.CreateAlert($"alert-{freq}", Rules.AnyChanges, Users.Xilapa.Id, frequency: freq);
            await AppFactory.WithDbContext(ctx => ctx.Set<Rule>()
                .Where(r => r.Id == alert.Rule.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.FirstWatchDone, true)));
        }
    }

    protected override void OnConfiguringTestServer(CustomWebApplicationOptionsBuilder optionsBuilder)
    {
        base.OnConfiguringTestServer(optionsBuilder);
        // TODO: Reconfigure Site to not be an owned type of Alert,
        // because it's not supported by SQLite, thus Postgres it's needed for this test
        optionsBuilder.UseDatabase(DatabaseType.PostgresOnDocker);
    }
}

public sealed class ExecuteAlertTests : BaseTest, IClassFixture<ExecuteAlertTestsBase>
{
    private readonly ExecuteAlertTestsBase _fixture;

    public ExecuteAlertTests(ExecuteAlertTestsBase fixture) : base(fixture)
    {
        _fixture = fixture;
        LoginAs(Users.Xilapa);
        FakePublisher.Messages.Clear();
        FakeCache.Cache.Clear();
    }

    [Theory]
    [InlineData(Rules.AnyChanges, false)]
    [InlineData(Rules.Term, false)]
    [InlineData(Rules.Regex, false)]
    [InlineData(Rules.Regex, true)]
    public async Task ExecuteAlertSucceeds(Rules rule, bool notifyOnDisappearance)
    {
        // Arrange
        var alertId = await CreateAlert(rule, notifyOnDisappearance);
        var alertCount = await AppFactory.WithDbContext(ctx => ctx.Alerts.CountAsync());
        SetupFakeHttpResponse(alertCount);

        // Create cache
        await GetAsync("alert");
        AppFactory.FakeCache.Cache.Count.Should().Be(1);

        // Alert should not be executed
        var alert = await GetAlertFromDb(alertId);
        alert.Rule.FirstWatchDone.Should().BeFalse();
        alert.LastVerification.Should().BeNull();

        // Act & Assert

        // First execution, alert rule should mark "first watch" as true
        await ExecuteAlerts();
        VerifyLogger(hasError: false);

        alert = await GetAlertFromDb(alertId);
        alert.Rule.FirstWatchDone.Should().BeTrue();
        alert.Triggerings.Should().BeEmpty();
        alert.LastVerification.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));

        // Alert should not be triggered
        var alertsTriggered = (FakePublisher.Messages.FirstOrDefault()?.Content as AlertsTriggeredEvent)?.Alerts;
        alertsTriggered?.Should().NotContain(a => a.AlertId == alertId);
        FakePublisher.Messages.Clear();

        // Cache should be cleared when alerts are executed
        AppFactory.FakeCache.Cache.Should().BeEmpty();

        // Second time
        SetupFakeHttpResponse(alertCount, baseMessage: "fake response fake response");
        CurrentTime = CurrentTime.Add(TimeSpan.FromMinutes(121));
        await ExecuteAlerts();
        VerifyLogger(hasError: false);

        // Alert should be updated
        alert = await GetAlertFromDb(alertId);
        alert.LastVerification.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));
        alert.LastUpdatedAt.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));

        // Alert should generate a triggering and publish a triggered event
        alertsTriggered = (FakePublisher.Messages.FirstOrDefault()?.Content as AlertsTriggeredEvent)?.Alerts;
        alertsTriggered?.Should().Contain(a => a.AlertId == alertId && a.Status.Equals(TriggeringStatus.Success));

        var triggering = alert.Triggerings.Single();
        triggering.Status.Should().Be(TriggeringStatus.Success);
        triggering.Date.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));
    }

    [Theory]
    [InlineData(Rules.AnyChanges, false)]
    [InlineData(Rules.Term, false)]
    [InlineData(Rules.Regex, false)]
    [InlineData(Rules.Regex, true)]
    public async Task ExecuteAlertFails(Rules rule, bool notifyOnDisappearance)
    {
        // Arrange
        LoginAs(Users.Xilapa);
        var alertCount = await AppFactory.WithDbContext(ctx => ctx.Alerts.CountAsync());
        SetupFakeHttpResponse(alertCount, errorResponse: true);
        var alertId = await CreateAlert(rule, notifyOnDisappearance);

        // Alert should not be executed
        var alert = await GetAlertFromDb(alertId);
        alert.Rule.FirstWatchDone.Should().BeFalse();
        alert.LastVerification.Should().BeNull();

        // Act
        await ExecuteAlerts();
        VerifyLogger(hasError: true);

        // Assert
        alert = await GetAlertFromDb(alertId);

        // Alert should be updated
        alert.LastVerification.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));
        alert.LastUpdatedAt.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));

        // Alert should generate an error triggering
        var triggering = alert.Triggerings.Single();
        triggering.Status.Should().Be(TriggeringStatus.Error);
        triggering.Date.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));

        // An error event should be published
        var alertsTriggered = (FakePublisher.Messages.FirstOrDefault()?.Content as AlertsTriggeredEvent)?.Alerts;
        alertsTriggered?.Should().Contain(a => a.AlertId == alertId && a.Status.Equals(TriggeringStatus.Error));
    }

    public static TheoryData<Stream, TimeSpan, bool> StreamData = new()
    {
        { Stream.Null, TimeSpan.FromMinutes(10), false },// Simulate an error reaching the site
        { new MemoryStream(new byte[] { 1, 2, 3 }), TimeSpan.FromMinutes(10), false },

        // After exactly two hours
        { Stream.Null, TimeSpan.FromHours(2),false },// Simulate an error reaching the site
        { new MemoryStream(new byte[] { 1, 2, 3 }), TimeSpan.FromHours(2), false },

        // After more than two hours
        { Stream.Null, TimeSpan.FromHours(3), true },// Simulate an error reaching the site
        { new MemoryStream(new byte[] { 1, 2, 3 }), TimeSpan.FromHours(3), true }
    };

    [Theory]
    [MemberData(nameof(StreamData))]
    public async Task AlertsAreReExecutedOnlyAfterTwoHours(Stream? siteStream, TimeSpan executionDelay, bool shouldExecute)
    {
        // Arrange

        // Create an alert and execute it
        var alert = await AppFactory.CreateAlert("alert", Rules.AnyChanges, Users.Xilapa.Id);
        await AppFactory.WithDbContext(async ctx =>
        {
            var alertFromDb = await ctx.Alerts
                .Include(a => a.Rule)
                .FirstAsync(a => a.Id == alert.Id);
            await alertFromDb.ExecuteRule(siteStream, AppFactory.CurrentTime);
            await ctx.SaveChangesAsync();
        });

        var frequencies = Enum.GetValues<Frequencies>().ToList();
        CurrentTime = CurrentTime.Add(executionDelay);

        // Act
        var usersWithAlerts = await AppFactory.WithDbContext(ctx =>
            ctx.GetUserWithPendingAlertsAsync(null, frequencies, 50, CurrentTime, CancellationToken.None));

        // Assert
        var alerts = usersWithAlerts.SelectMany(u => u.Alerts);
        alerts.Any(a => a.Id == alert.Id).Should().Be(shouldExecute);
    }

    public static TheoryData<List<Frequencies>> Freqs = new()
    {
        new List<Frequencies> {Frequencies.TwoHours },
        new List<Frequencies> {Frequencies.FourHours},
        new List<Frequencies> {Frequencies.EightHours},
        new List<Frequencies> {Frequencies.TwelveHours},
        new List<Frequencies> {Frequencies.TwentyFourHours},
        new List<Frequencies> {Frequencies.TwoHours, Frequencies.FourHours},
        new List<Frequencies> {Frequencies.TwoHours, Frequencies.FourHours, Frequencies.EightHours},
        new List<Frequencies> {Frequencies.TwoHours, Frequencies.FourHours, Frequencies.EightHours, Frequencies.TwelveHours},
        new List<Frequencies> {Frequencies.TwoHours, Frequencies.FourHours, Frequencies.EightHours, Frequencies.TwelveHours, Frequencies.TwentyFourHours}
    };

    [Theory]
    [MemberData(nameof(Freqs))]
    public async Task AlertsAreOnlyExecutedOnTheirFrequency(List<Frequencies> freq)
    {
        // Arrange
        var alertCount = await AppFactory.WithDbContext(ctx => ctx.Alerts.CountAsync());
        SetupFakeHttpResponse(count: alertCount);
        CurrentTime = CurrentTime.Add(TimeSpan.FromMinutes(121));

        // Act
        await ExecuteAlerts(freq);

        // Assert
        var alertsTriggeredEvent = FakePublisher.Messages.SingleOrDefault()?.Content as AlertsTriggeredEvent;
        alertsTriggeredEvent!.Alerts.Should().OnlyContain(a => freq.Contains(a.Frequency));

        var currentAlertsTriggeredFromDb = await AppFactory.WithDbContext(ctx =>
                ctx.Alerts.Where(a => a.LastVerification == CurrentTime).ToArrayAsync());

        currentAlertsTriggeredFromDb.Should().OnlyContain(a => freq.Contains(a.Frequency));
    }

    private void SetupFakeHttpResponse(int count, bool errorResponse = false, string? baseMessage = null)
    {
        var exception = new HttpRequestException();
        var fakeResponses = Enumerable.Range(0, count)
            .Select(i =>
                errorResponse ?
                    new FakeHttpResponse { StatusCode = HttpStatusCode.InternalServerError, Exception = exception} :
                    new FakeHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Response = baseMessage is null ? $"fake response {i}" : $"{baseMessage} {i}"
                    }
            )
            .ToArray();

        var fakeDelegateHandler = new FakeHttpDelegateHandler(fakeResponses);

        HttpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(new HttpClient(fakeDelegateHandler));
    }

    private async Task<AlertId> CreateAlert(Rules rule, bool notifyOnDisappearance)
    {
        var alertId = (await AppFactory.CreateAlert("alert", Rules.AnyChanges, Users.Xilapa.Id)).Id;
        var cmmd = new UpdateAlertCommmand
        {
            AlertId = IdHasher.HashId(alertId.Value),
            Rule = new UpdateInfo<Rules>(rule),
            Term = new UpdateInfo<string>("fake response"),
            NotifyOnDisappearance = new UpdateInfo<bool>(notifyOnDisappearance),
            RegexPattern = new UpdateInfo<string>("[0-9]")
        };

        var result = await PutAsync("alert", cmmd);
        result.HttpResponse!.StatusCode.Should().Be(HttpStatusCode.OK);

        // Clear triggerings
        await AppFactory.WithDbContext(ctx =>
                ctx.Set<Triggering>().ExecuteDeleteAsync());

        return alertId;
    }

    private Task<Alert> GetAlertFromDb(AlertId alertId)
    {
        return AppFactory.WithDbContext(ctx =>
                ctx.Alerts
                .Where(a => a.Id == alertId)
                .Include(a => a.Rule)
                .Include(a => a.Triggerings)
                .AsNoTracking()
                .FirstAsync()
        );
    }

    private async Task ExecuteAlerts(List<Frequencies>? frequencies = null)
    {
        var executeAlertsCmmd = new ExecuteAlertsCommand(frequencies ?? Enum.GetValues<Frequencies>().ToList());
        await AppFactory.WithServiceProvider(async sp =>
        {
            var scope = sp.CreateAsyncScope();
            var executeAlertsCommandHandler = scope.ServiceProvider.GetRequiredService<ExecuteAlertsCommandHandler>();
            await executeAlertsCommandHandler.Handle(executeAlertsCmmd, CancellationToken.None);
        });
    }

    private void VerifyLogger(bool hasError)
    {
        var times = hasError ? Times.AtLeastOnce() : Times.Never();
        var failMessage = hasError ? "Alerts executed successfully" : "Alerts executed with errors";
        LoggerMock.Verify(logger =>
            logger.Log(It.Is<LogLevel>(l => LogLevel.Error.Equals(l)),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType,Exception?,string>>((_,_) => true)),
            times, failMessage);
    }
}