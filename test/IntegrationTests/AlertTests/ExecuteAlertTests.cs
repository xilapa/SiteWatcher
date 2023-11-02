using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Domain.Alerts.Entities.Triggerings;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.Messages;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.Persistence;
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
            var alert = await AppFactory.CreateAlert($"alert-{freq}", RuleType.AnyChanges, Users.Xilapa.Id, frequency: freq);
            await AppFactory.WithDbContext(ctx => ctx.Set<Rule>()
                .Where(r => r.Id == alert.Rule.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.FirstWatchDone, true)));
        }
    }

    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        base.OnConfiguringTestServer(optionsBuilder);
        // TODO: Reconfigure Site to not be an owned type of Alert,
        // because it's not supported by SQLite, thus Postgres it's needed for this test
        optionsBuilder.UseDatabase(DatabaseType.Postgres);
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
    [InlineData(RuleType.AnyChanges, false)]
    [InlineData(RuleType.Term, false)]
    [InlineData(RuleType.Regex, false)]
    [InlineData(RuleType.Regex, true)]
    public async Task ExecuteAlertSucceeds(RuleType ruleType, bool notifyOnDisappearance)
    {
        // Arrange
        var alertId = await CreateAlert(ruleType, notifyOnDisappearance);
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

        alert = await GetAlertFromDb(alertId);
        alert.Rule.FirstWatchDone.Should().BeTrue();
        alert.Triggerings.Should().BeEmpty();
        alert.LastVerification.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));

        // Alert should not be triggered
        var alertsTriggered = (FakePublisher.Messages.FirstOrDefault()?.Content as AlertsTriggeredMessage)?.Alerts;
        alertsTriggered?.Should().NotContain(a => a.AlertId == alertId);
        FakePublisher.Messages.Clear();

        if (notifyOnDisappearance)
            (alert.Rule as RegexRule)!.Matches.Count.Should().Be(1);

        // Cache should be cleared when alerts are executed
        AppFactory.FakeCache.Cache.Should().BeEmpty();

        // Second time
        SetupFakeHttpResponse(alertCount, baseMessage: "fake response fake response", removeCounterFromMessage: notifyOnDisappearance);
        CurrentTime = CurrentTime.Add(TimeSpan.FromMinutes(121));
        await ExecuteAlerts();

        // Alert should be updated
        alert = await GetAlertFromDb(alertId);
        alert.LastVerification.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));
        alert.LastUpdatedAt.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));

        if (notifyOnDisappearance)
            (alert.Rule as RegexRule)!.Matches.Should().BeEmpty();

        // Alert should generate a triggering and publish a triggered event
        alertsTriggered = (FakePublisher.Messages.FirstOrDefault()?.Content as AlertsTriggeredMessage)?.Alerts;
        alertsTriggered?.Should().Contain(a => a.AlertId == alertId && a.Status.Equals(TriggeringStatus.Success));

        var triggering = alert.Triggerings.Single();
        triggering.Status.Should().Be(TriggeringStatus.Success);
        triggering.Date.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));
    }

    [Theory]
    [InlineData(RuleType.AnyChanges, false)]
    [InlineData(RuleType.Term, false)]
    [InlineData(RuleType.Regex, false)]
    [InlineData(RuleType.Regex, true)]
    public async Task ExecuteAlertFails(RuleType ruleType, bool notifyOnDisappearance)
    {
        // Arrange
        LoginAs(Users.Xilapa);
        var alertCount = await AppFactory.WithDbContext(ctx => ctx.Alerts.CountAsync());
        SetupFakeHttpResponse(alertCount, errorResponse: true);
        var alertId = await CreateAlert(ruleType, notifyOnDisappearance);

        // Alert should not be executed
        var alert = await GetAlertFromDb(alertId);
        alert.Rule.FirstWatchDone.Should().BeFalse();
        alert.LastVerification.Should().BeNull();

        // Act
        await ExecuteAlerts();

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
        var alertsTriggered = (FakePublisher.Messages.FirstOrDefault()?.Content as AlertsTriggeredMessage)?.Alerts;
        alertsTriggered?.Should().Contain(a => a.AlertId == alertId && a.Status.Equals(TriggeringStatus.Error));
    }

    public static TheoryData<Stream, TimeSpan, bool> StreamData = new()
    {
        { Stream.Null, TimeSpan.FromMinutes(10), false },// Simulate an error reaching the site
        { new MemoryStream(new byte[] { 1, 2, 3 }), TimeSpan.FromMinutes(10), false },

        { Stream.Null, TimeSpan.FromMinutes(119), false },// Simulate an error reaching the site
        { new MemoryStream(new byte[] { 1, 2, 3 }), TimeSpan.FromMinutes(119), false },

        // After exactly two hours
        { Stream.Null, TimeSpan.FromHours(2), true },// Simulate an error reaching the site
        { new MemoryStream(new byte[] { 1, 2, 3 }), TimeSpan.FromHours(2), true },

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
        var alert = await AppFactory.CreateAlert("alert", RuleType.AnyChanges, Users.Xilapa.Id);
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
        var alertsTriggeredEvent = FakePublisher.Messages.SingleOrDefault()?.Content as AlertsTriggeredMessage;
        alertsTriggeredEvent!.Alerts.Should().OnlyContain(a => freq.Contains(a.Frequency));

        var currentAlertsTriggeredFromDb = await AppFactory.WithDbContext(ctx =>
                ctx.Alerts.Where(a => a.LastVerification == CurrentTime).ToArrayAsync());

        currentAlertsTriggeredFromDb.Should().OnlyContain(a => freq.Contains(a.Frequency));
    }

    private void SetupFakeHttpResponse(int count, bool errorResponse = false, string? baseMessage = null,
        bool? removeCounterFromMessage = false)
    {
        var exception = new HttpRequestException();
        var fakeResponses = Enumerable.Range(0, count)
            .Select(i =>
                errorResponse ?
                    new FakeHttpResponse { StatusCode = HttpStatusCode.InternalServerError, Exception = exception} :
                    new FakeHttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        Response = baseMessage is null ?
                            $"fake response {(removeCounterFromMessage!.Value ? string.Empty : i)}" :
                            $"{baseMessage} {(removeCounterFromMessage!.Value ? string.Empty : i)}"
                    }
            )
            .ToArray();

        var fakeDelegateHandler = new FakeHttpDelegateHandler(fakeResponses);

        HttpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(new HttpClient(fakeDelegateHandler));
    }

    private async Task<AlertId> CreateAlert(RuleType ruleType, bool notifyOnDisappearance)
    {
        var alertId = (await AppFactory.CreateAlert("alert", RuleType.AnyChanges, Users.Xilapa.Id)).Id;
        var cmmd = new UpdateAlertCommmand
        {
            AlertId = IdHasher.HashId(alertId.Value),
            RuleType = new UpdateInfo<RuleType>(ruleType),
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
}