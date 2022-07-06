using System.Net;
using Domain.DTOs.Common;
using FluentAssertions;
using HashidsNet;
using IntegrationTests.Setup;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Domain.Enums;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace IntegrationTests.AlertTests;

public class GetAlertsTestsBase : BaseTestFixture
{
    public static SimpleAlertView[] XilapaAlerts { get; set; } = null!;
    public static DateTime StartingTime { get; } = new(2020, 1, 1, 0, 0, 0);

    public override Action<CustomWebApplicationOptions> Options =>
        opts => opts.DatabaseType = DatabaseType.SqliteOnDisk;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        XilapaAlerts = new SimpleAlertView[20];
        AppFactory.CurrentTime = StartingTime;
        for (var i = 0; i < 20; i++)
        {
            var watchMode = i > 9 ? EWatchMode.Term : EWatchMode.AnyChanges;
            AppFactory.CurrentTime = AppFactory.CurrentTime.AddMinutes(5);

            // xilapa'll have odd alert ids
            var alertXilapa = await AppFactory
                .CreateAlert<SimpleAlertView>($"alert{(2 * (i + 1)) - 1}", watchMode, Users.Xilapa.Id, AppFactory.CurrentTime);
            XilapaAlerts[i] = alertXilapa;

            // xulipa'll have even alert ids
            await AppFactory
                .CreateAlert<SimpleAlertView>($"alert{2 * (i + 1)}", watchMode, Users.Xulipa.Id, AppFactory.CurrentTime);
        }
    }
}

public class GetAlertsTests : BaseTest, IClassFixture<GetAlertsTestsBase>
{
    public GetAlertsTests(GetAlertsTestsBase fixture) : base(fixture)
    {
        FakeCache.Cache.Clear();
    }

    [Fact]
    public async Task GetAlertsOnlyForTheLoggedInUser()
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Act
        var result = await GetAsync("alert");

        // Assert
        result.HttpResponse!
            .StatusCode
            .Should().Be(HttpStatusCode.OK);

        result.GetTyped<WebApiResponse<PaginatedList<SimpleAlertView>>>()!
            .Result!.Results.Should()
            .BeEquivalentTo(GetAlertsTestsBase.XilapaAlerts.Take(10),
                opt => opt.WithoutStrictOrdering());
    }

    public static IEnumerable<object[]> PaginationData()
    {
        yield return new object[]
        {
            new GetUserAlertsCommand
            {
                Take = 10,
                LastAlertId = null
            },
            GetAlertsTestsBase.StartingTime,
            1,
            (2 * 10) - 1, // The id of the tenth xilapa's alert
            10
        };

        yield return new object[]
        {
            new GetUserAlertsCommand
            {
                Take = 10,
                LastAlertId = new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength)
                    .Encode((2 * 10) - 1),
            },
            GetAlertsTestsBase.StartingTime.AddMinutes(10 * 5), // The creation date of the tenth xilapa's alert
            (2 * 11) - 1, // The id of the eleventh xilapa's alert
            (2 * 20) - 1, // The id of the twentieth xilapa's alert
            10
        };

        yield return new object[]
        {
            new GetUserAlertsCommand
            {
                Take = 10,
                LastAlertId = new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength)
                    .Encode((2 * 20) - 1),
            },
            GetAlertsTestsBase.StartingTime.AddMinutes(20 * 5), // The creation date of the twentieth xilapa's alert
            (2 * 11) - 1, // The id of the eleventh xilapa's alert
            (2 * 20) - 1, // The id of the twenty-first xilapa's alert
            0
        };
    }

    [Theory]
    [MemberData(nameof(PaginationData))]
    public async Task PaginationTests(GetUserAlertsCommand command, DateTime dateToFilter, int firstid, int lastId, int count)
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Act
        var result = await GetAsync("alert", command);

        // Assert
        result.HttpResponse!
            .StatusCode
            .Should().Be(HttpStatusCode.OK);

        command.Take = command.Take == 0 ? 10 : command.Take;

        var expected = GetAlertsTestsBase.XilapaAlerts
            .Where(a => a.CreatedAt > dateToFilter)
            .Take(command.Take)
            .ToArray();

        var typedResult = result
            .GetTyped<WebApiResponse<PaginatedList<SimpleAlertView>>>()!
            .Result!.Results.ToArray();

        typedResult.Length.Should().Be(count);
        if (count != 0)
        {
            typedResult[0].Name.Should().Contain(firstid.ToString());
            typedResult[command.Take-1].Name.Should().Contain(lastId.ToString());
        }

        typedResult.Should().BeEquivalentTo(expected,
            opt => opt.WithStrictOrdering());
    }

    public static IEnumerable<object[]> TakeData()
    {
        yield return new object[]
        {
            new GetUserAlertsCommand
            {
                Take = 15,
                LastAlertId = null
            },
            15
        };

        yield return new object[]
        {
            new GetUserAlertsCommand
            {
                Take = 50,
                LastAlertId = null
            },
            GetAlertsTestsBase.XilapaAlerts.Length
        };

        yield return new object[]
        {
            new GetUserAlertsCommand
            {
                Take = 51,
                LastAlertId = null
            },
            GetAlertsTestsBase.XilapaAlerts.Length
        };
    }

    [Theory]
    [MemberData(nameof(TakeData))]
    public async Task CantTakeMoreThan50Results(GetUserAlertsCommand command, int count)
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Act
        var result = await GetAsync("alert", command);

        // Assert
        result.HttpResponse!
            .StatusCode
            .Should().Be(HttpStatusCode.OK);

        result
            .GetTyped<WebApiResponse<PaginatedList<SimpleAlertView>>>()!
            .Result!.Results.Count().Should().Be(count);
    }
}