using System.Net;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Setup;
using Moq;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AlertTests;

public sealed class AlertCacheTestsBase : BaseTestFixture
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var alertToUpdate = await AppFactory.CreateAlert("alert", Rules.Term, Users.Xilapa.Id);
        AlertToUpdateId = alertToUpdate.Id;
        var alertToDelete = await AppFactory.CreateAlert("alert", Rules.Term, Users.Xilapa.Id);
        AlertToDeleteId = alertToDelete.Id;
    }

    public Mock<IIdHasher> IdHasherMock { get; private set; }
    public AlertId AlertToUpdateId { get; private set; }
    public AlertId AlertToDeleteId { get; private set; }

    protected override void OnConfiguringTestServer(CustomWebApplicationOptionsBuilder optionsBuilder)
    {
        // Replace services to delete alert test
        IdHasherMock = new Mock<IIdHasher>();
        IdHasherMock
            .Setup(i => i.DecodeId(It.IsAny<string>()))
            .Returns(AlertToUpdateId.Value);

        optionsBuilder.ReplaceService(typeof(IIdHasher), IdHasherMock.Object);
        optionsBuilder.UseDatabase(DatabaseType.SqliteOnDisk);
    }
}

public class AlertCacheTests : BaseTest, IClassFixture<AlertCacheTestsBase>
{
    private readonly AlertCacheTestsBase _fixture;

    public AlertCacheTests(AlertCacheTestsBase fixture) : base(fixture)
    {
        _fixture = fixture;
        _fixture.IdHasherMock.Invocations.Clear();
        FakeCache.Cache.Clear();
    }

    [Fact]
    public async Task CacheIsCalledOnConsecutiveRequest()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        FakeCache.Cache.Count.Should().Be(0);
        // Decode lastAlertId to zero
        _fixture.IdHasherMock.Setup(h => h.DecodeId(It.IsAny<string>()))
            .Returns(0);

        // Act and Assert

        // first call
        await GetAsync("alert");
        FakeCache.Cache.Count.Should().Be(1);
        var cachedResult = JsonSerializer
            .Deserialize<PaginatedList<SimpleAlertView>>(
                (FakeCache.Cache.Values.First().Value as string)!);

        cachedResult!.Results.Length.Should().BeGreaterThan(0);
        _fixture.IdHasherMock.Invocations.Clear();

        // second call
        var result = await GetAsync("alert");

        // Id should not be decoded
        _fixture.IdHasherMock.Verify(i => i.DecodeId(It.IsAny<string>()), Times.Never);
        result.GetTyped<PaginatedList<SimpleAlertView>>()! // result must be the same
            .Results.Should().BeEquivalentTo(cachedResult.Results);
        FakeCache.Cache.Count.Should().Be(1); // fake cache must have the same entry count
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
        // Decode lastAlertId to zero
        _fixture.IdHasherMock.Setup(h => h.DecodeId(It.IsAny<string>()))
            .Returns(0);

        // Act and Assert

        // get alerts
        await GetAsync("alert");
        FakeCache.Cache.Count.Should().Be(1);
        var cachedResult = JsonSerializer
            .Deserialize<PaginatedList<SimpleAlertView>>(
                (FakeCache.Cache.Values.First().Value as string)!);

        cachedResult!.Results.Length.Should().BeGreaterThan(0);

        // create alert
        var result = await PostAsync("alert", createAlertCommand);

        result.HttpResponse!.StatusCode
            .Should().Be(HttpStatusCode.Created);

        FakeCache.Cache.Should().BeEmpty();
    }

    [Fact]
    public async Task CacheIsRemovedWhenAlertIsDeleted()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        FakeCache.Cache.Should().BeEmpty();
        _fixture.IdHasherMock.Setup(h => h.DecodeId(It.IsAny<string>()))
            .Returns(_fixture.AlertToDeleteId.Value);

        // create cache
        await GetAsync("alert");
        FakeCache.Cache.Should().NotBeEmpty();

        // Act
        var res = await DeleteAsync("alert/mocked");
        res.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

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
        _fixture.IdHasherMock.Setup(h => h.DecodeId(It.IsAny<string>()))
            .Returns(_fixture.AlertToUpdateId.Value);

        // create cache
        await GetAsync("alert");
        FakeCache.Cache.Should().NotBeEmpty();

        // Act
        var res = await PutAsync("alert", updateCommand);
        res.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        FakeCache.Cache.Should().BeEmpty();
    }
}