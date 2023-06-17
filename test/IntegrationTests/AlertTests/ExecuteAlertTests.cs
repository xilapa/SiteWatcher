using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SiteWatcher.Application.Alerts.Commands.ExecuteAlerts;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Entities.Triggerings;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AlertTests;

public sealed class ExecuteAlertTestsBase : BaseTestFixture
{
    internal AlertId AlertId { get; private set; }
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var alert = await AppFactory.CreateAlert("alert", Rules.AnyChanges, Users.Xilapa.Id);
        AlertId = alert.Id;
    }

    protected override void OnConfiguringTestServer(CustomWebApplicationOptionsBuilder optionsBuilder)
    {
        base.OnConfiguringTestServer(optionsBuilder);
        // TODO: Reconfigure Site to not be an owned type of Alert,
        // because it's not supported by SQLite, thus Postgres it's needed for this test
        optionsBuilder.UseDatabase(DatabaseType.PostgresOnDocker);
    }
}

public class ExecuteAlertTests : BaseTest, IClassFixture<ExecuteAlertTestsBase>
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
    public async Task ExecuteAnyChangesAlertSucceeds(Rules rule, bool notifyOnDisappearance)
    {
        // Arrange
        SetupFakeHttpResponse();
        await UpdateAlert(rule, notifyOnDisappearance);
        // Create cache
        await GetAsync("alert");
        AppFactory.FakeCache.Cache.Count.Should().Be(1);

        // Alert should not be executed
        var alert = await GetAlertFromDb();
        alert.Rule.FirstWatchDone.Should().BeFalse();
        alert.LastVerification.Should().BeNull();

        // Act & Assert

        // First execution, alert rule should mark "first watch" as true
        var result = await ExecuteAlerts();
        result.Value.Should().BeTrue();
        alert = await GetAlertFromDb();
        alert.Rule.FirstWatchDone.Should().BeTrue();
        FakePublisher.Messages.Should().BeEmpty();
        alert.LastVerification.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));

        // Cache should be cleared when alerts are executed
        AppFactory.FakeCache.Cache.Should().BeEmpty();

        // Second time
        result = await ExecuteAlerts();
        result.Value.Should().BeTrue();

        // Alert should be updated
        alert = await GetAlertFromDb();
        alert.LastVerification.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));
        alert.LastUpdatedAt.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));

        // Alert should generate a triggering and publish a triggered event
        FakePublisher.Messages.Should().HaveCount(1);
        (FakePublisher.Messages[0].Message as AlertsTriggeredEvent)!.
            Alerts.Should().OnlyContain(a => a.Status.Equals(TriggeringStatus.Success));

        var triggering = alert.Triggerings.Single();
        triggering.Status.Should().Be(TriggeringStatus.Success);
        triggering.Date.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));
    }

    [Theory]
    [InlineData(Rules.AnyChanges, false)]
    [InlineData(Rules.Term, false)]
    [InlineData(Rules.Regex, false)]
    [InlineData(Rules.Regex, true)]
    public async Task ExecuteAnyChangesAlertFails(Rules rule, bool notifyOnDisappearance)
    {
        // Arrange
        LoginAs(Users.Xilapa);
        SetupFakeHttpResponse(errorResponse: true);
        await UpdateAlert(rule, notifyOnDisappearance);

        // Alert should not be executed
        var alert = await GetAlertFromDb();
        alert.Rule.FirstWatchDone.Should().BeFalse();
        alert.LastVerification.Should().BeNull();

        // Act & Assert
        var result = await ExecuteAlerts();
        result.Value.Should().BeTrue();
        alert = await GetAlertFromDb();

        // Alert should be updated but not the last verification date
        alert.LastVerification.Should().BeNull();
        alert.LastUpdatedAt.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));

        // Alert should generate a triggering and publish a triggered event
        FakePublisher.Messages.Should().HaveCount(1);
        (FakePublisher.Messages[0].Message as AlertsTriggeredEvent)!.
            Alerts.Should().OnlyContain(a => a.Status.Equals(TriggeringStatus.Error));

        var triggering = alert.Triggerings.Single();
        triggering.Status.Should().Be(TriggeringStatus.Error);
        triggering.Date.Should().BeCloseTo(AppFactory.CurrentTime, TimeSpan.FromMilliseconds(1));
    }

    private void SetupFakeHttpResponse(bool errorResponse = false)
    {
        var fakeDelegateHandler = new FakeHttpDelegateHandler(
            new FakeHttpResponse { StatusCode = HttpStatusCode.OK, Response = "fake response 1" },
            new FakeHttpResponse { StatusCode = HttpStatusCode.OK, Response = "fake response 2" }
        );

        if (errorResponse)
        {
            var exception = new HttpRequestException();
            fakeDelegateHandler = new FakeHttpDelegateHandler(
                new FakeHttpResponse { StatusCode = HttpStatusCode.InternalServerError, Exception = exception},
                new FakeHttpResponse { StatusCode = HttpStatusCode.InternalServerError, Exception = exception},
                new FakeHttpResponse { StatusCode = HttpStatusCode.InternalServerError, Exception = exception}
            );
        }
        HttpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName))
            .Returns(new HttpClient(fakeDelegateHandler));
    }

    private async Task UpdateAlert(Rules rule, bool notifyOnDisappearance)
    {
        var cmmd = new UpdateAlertCommmand
        {
            AlertId = new IdHasher(new TestAppSettings()).HashId(_fixture.AlertId.Value),
            Rule = new UpdateInfo<Rules>(rule),
            Term = new UpdateInfo<string>("fake response 2"),
            NotifyOnDisappearance = new UpdateInfo<bool>(notifyOnDisappearance),
            RegexPattern = new UpdateInfo<string>("[0-9]")
        };

        if (notifyOnDisappearance) cmmd.Term = new UpdateInfo<string>("fake response 1");

        var result = await PutAsync("alert", cmmd);
        result.HttpResponse!.StatusCode.Should().Be(HttpStatusCode.OK);

        // Clear triggerings
        await AppFactory.WithDbContext(ctx =>
        {
            return ctx.Set<Triggering>()
                .Where(_ => true)
                .ExecuteDeleteAsync();
        });
    }

    private Task<Alert> GetAlertFromDb()
    {
        return AppFactory.WithDbContext(ctx =>
                ctx.Alerts
                .Include(a => a.Rule)
                .Include(a => a.Triggerings)
                .AsNoTracking()
                .FirstAsync()
        );
    }

    private async Task<ValueResult<bool>> ExecuteAlerts()
    {
        var executeAlertsCmmd = new ExecuteAlertsCommand(Enum.GetValues<Frequencies>());
        return await AppFactory.WithServiceProvider(async sp =>
        {
            var scope = sp.CreateAsyncScope();
            var executeAlertsCommandHandler = scope.ServiceProvider.GetRequiredService<ExecuteAlertsCommandHandler>();
            var res = await executeAlertsCommandHandler.Handle(executeAlertsCmmd, CancellationToken.None);
            return (res as ValueResult<bool>)!;
        });
    }
}