using System.Net;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Setup;
using MediatR;
using Moq;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Domain.Enums;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace IntegrationTests.AlertTests;

public class GetAlertCacheTestsBase : BaseTestFixture
{
    public Mock<IRequestHandler<GetUserAlertsCommand, ICommandResult<IEnumerable<SimpleAlertView>>>> CommandHandlerMock
    {
        get;
        set;
    }

    public SimpleAlertView[] AlertsView { get; set; }

    public override Action<CustomWebApplicationOptions> Options => opt =>
    {
        AlertsView = new SimpleAlertView[]
        {
            new()
            {
                Id = "id1",
                Frequency = EFrequency.TwelveHours,
                Name = "alert1",
                CreatedAt = DateTime.UtcNow,
                SiteName = "siteName1",
                WatchMode = EWatchMode.Term
            },
            new()
            {
                Id = "id2",
                Frequency = EFrequency.TwentyFourHours,
                Name = "alert2",
                CreatedAt = DateTime.UtcNow,
                SiteName = "siteName2",
                WatchMode = EWatchMode.AnyChanges
            },
            new()
            {
                Id = "id3",
                Frequency = EFrequency.FourHours,
                Name = "alert3",
                CreatedAt = DateTime.UtcNow.AddMinutes(6),
                SiteName = "siteName3",
                WatchMode = EWatchMode.Term
            }
        };

        CommandHandlerMock =
            new Mock<IRequestHandler<GetUserAlertsCommand, ICommandResult<IEnumerable<SimpleAlertView>>>>();

        CommandHandlerMock
            .Setup(h =>
                h.Handle(It.IsAny<GetUserAlertsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommandResult<IEnumerable<SimpleAlertView>>(AlertsView));

        opt.ReplaceService(typeof(IRequestHandler<GetUserAlertsCommand, ICommandResult<IEnumerable<SimpleAlertView>>>),
            CommandHandlerMock.Object);
    };
}

public class GetAlertCacheTests : BaseTest, IClassFixture<GetAlertCacheTestsBase>
{
    private readonly GetAlertCacheTestsBase _fixture;

    public GetAlertCacheTests(GetAlertCacheTestsBase fixture) : base(fixture)
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
            .Deserialize<WebApiResponse<IEnumerable<SimpleAlertView>>>((FakeCache.Cache.Values.First().Value as string)!);

        cachedResult!.Result.Should().BeEquivalentTo(_fixture.AlertsView);

        _fixture.CommandHandlerMock
            .Verify(c =>
                c.Handle(It.IsAny<GetUserAlertsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _fixture.CommandHandlerMock.Invocations.Clear();

        // second call
        var result = await GetAsync("alert");
        result.GetTyped<WebApiResponse<IEnumerable<SimpleAlertView>>>()!. // result must be the same
            Result.Should().BeEquivalentTo(_fixture.AlertsView);
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
        var createAlertCommand = new CreateAlertCommand()
        {
            Frequency = EFrequency.EightHours,
            Name = "name",
            WatchMode = EWatchMode.AnyChanges,
            SiteName = "site",
            SiteUri = "http://site.test.io"
        };

        // Act and Assert

        // get alerts
        await GetAsync("alert");
        FakeCache.Cache.Count.Should().Be(1);
        var cachedResult = JsonSerializer
            .Deserialize<WebApiResponse<IEnumerable<SimpleAlertView>>>((FakeCache.Cache.Values.First().Value as string)!);

        cachedResult!.Result.Should().BeEquivalentTo(_fixture.AlertsView);

        // create alert
        var result = await PostAsync("alert", createAlertCommand);

        result.HttpResponse!.StatusCode
            .Should().Be(HttpStatusCode.OK);

        // Await fire and forget to execute
        await Task.Delay(300);

        FakeCache.Cache.Count.Should().Be(0);
    }
}