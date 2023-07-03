using System.Net;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Setup;
using MediatR;
using Moq;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AlertTests;

public sealed class AlertCacheTestsBase : BaseTestFixture
{
    public AlertCacheTestsBase()
    {
        CommandHandlerMock = new Mock<IRequestHandler<GetUserAlertsCommand, CommandResult>>();
        AlertsView = new SimpleAlertView[]
        {
            new()
            {
                Id = "id1",
                Frequency = Frequencies.TwelveHours,
                Name = "alert1",
                CreatedAt = DateTime.UtcNow,
                SiteName = "siteName1",
                Rule = Rules.Term
            },
            new()
            {
                Id = "id2",
                Frequency = Frequencies.TwentyFourHours,
                Name = "alert2",
                CreatedAt = DateTime.UtcNow,
                SiteName = "siteName2",
                Rule = Rules.AnyChanges
            },
            new()
            {
                Id = "id3",
                Frequency = Frequencies.FourHours,
                Name = "alert3",
                CreatedAt = DateTime.UtcNow.AddMinutes(6),
                SiteName = "siteName3",
                Rule = Rules.Term
            }
        };
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var alertToUpdate = await AppFactory.CreateAlert("alert", Rules.Term, Users.Xilapa.Id);
        AlertToUpdateId = alertToUpdate.Id;
    }

    public Mock<IRequestHandler<GetUserAlertsCommand, CommandResult>> CommandHandlerMock { get; }
    public SimpleAlertView[] AlertsView { get; }
    public AlertId AlertToUpdateId { get; private set; }

    protected override void OnConfiguringTestServer(CustomWebApplicationOptionsBuilder optionsBuilder)
    {
        CommandHandlerMock
            .Setup(h =>
                h.Handle(It.IsAny<GetUserAlertsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandResult.FromValue(new PaginatedList<SimpleAlertView>(AlertsView, 0)));

        optionsBuilder
            .ReplaceService(typeof(IRequestHandler<GetUserAlertsCommand, CommandResult>), CommandHandlerMock.Object);

        // Replace services to delete alert test
        var idHasherMock = new Mock<IIdHasher>();
        idHasherMock
            .Setup(i => i.DecodeId(It.IsAny<string>()))
            .Returns(1);

        optionsBuilder.ReplaceService(typeof(IIdHasher), idHasherMock.Object);
    }
}

public class AlertCacheTests : BaseTest, IClassFixture<AlertCacheTestsBase>
{
    private readonly AlertCacheTestsBase _fixture;

    public AlertCacheTests(AlertCacheTestsBase fixture) : base(fixture)
    {
        _fixture = fixture;
        _fixture.CommandHandlerMock.Invocations.Clear();
        FakeCache.Cache.Clear();
    }

    [Fact]
    public async Task CacheIsCalledOnConsecutiveRequest()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        FakeCache.Cache.Count.Should().Be(0);

        // Act and Assert

        // first call
        await GetAsync("alert");
        FakeCache.Cache.Count.Should().Be(1);
        var cachedResult = JsonSerializer
            .Deserialize<PaginatedList<SimpleAlertView>>(
                (FakeCache.Cache.Values.First().Value as string)!);

        cachedResult!.Results.Should().BeEquivalentTo(_fixture.AlertsView);

        _fixture.CommandHandlerMock
            .Verify(c =>
                c.Handle(It.IsAny<GetUserAlertsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _fixture.CommandHandlerMock.Invocations.Clear();

        // second call
        var result = await GetAsync("alert");
        result.GetTyped<PaginatedList<SimpleAlertView>>()! // result must be the same
            .Results.Should().BeEquivalentTo(_fixture.AlertsView);
        FakeCache.Cache.Count.Should().Be(1); // fake cache must have the same entry
        _fixture.CommandHandlerMock // handler should not be called
            .Verify(c =>
                c.Handle(It.IsAny<GetUserAlertsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CacheIsRemovedWhenAlertIsCreated()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        FakeCache.Cache.Count.Should().Be(0);
        var createAlertCommand = new CreateAlertCommand
        {
            Frequency = Frequencies.EightHours,
            Name = "name",
            Rule = Rules.AnyChanges,
            SiteName = "site",
            SiteUri = "http://site.test.io"
        };

        // Act and Assert

        // get alerts
        await GetAsync("alert");
        FakeCache.Cache.Count.Should().Be(1);
        var cachedResult = JsonSerializer
            .Deserialize<PaginatedList<SimpleAlertView>>(
                (FakeCache.Cache.Values.First().Value as string)!);

        cachedResult!.Results.Should().BeEquivalentTo(_fixture.AlertsView);

        // create alert
        var result = await PostAsync("alert", createAlertCommand);

        result.HttpResponse!.StatusCode
            .Should().Be(HttpStatusCode.Created);

        // Await fire and forget to execute
        await Task.Delay(300);

        FakeCache.Cache.Should().BeEmpty();
    }

    [Fact]
    public async Task CacheIsRemovedWhenAlertIsDeleted()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        FakeCache.Cache.Should().BeEmpty();

        // create cache
        await GetAsync("alert");
        FakeCache.Cache.Should().NotBeEmpty();

        // Act
        var res = await DeleteAsync("alert/mocked");
        res.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Await fire and forget to execute
        await Task.Delay(300);

        // Assert
        FakeCache.Cache.Should().BeEmpty();
    }

    [Fact]
    public async Task CacheIsRemovedWhenAlertIsUpdated()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        FakeCache.Cache.Should().BeEmpty();
        var updateCommand = new UpdateAlertCommmand
        {
            AlertId = new IdHasher(new TestAppSettings()).HashId(_fixture.AlertToUpdateId.Value),
            Name = new UpdateInfo<string> {NewValue = "new name"}
        };

        // create cache
        await GetAsync("alert");
        FakeCache.Cache.Should().NotBeEmpty();

        // Act
        var res = await PutAsync("alert", updateCommand);
        res.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.OK);

        // Await fire and forget to execute
        await Task.Delay(300);

        // Assert
        FakeCache.Cache.Should().BeEmpty();
    }
}