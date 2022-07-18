using System.Net;
using AutoMapper;
using Domain.DTOs.Common;
using FluentAssertions;
using HashidsNet;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Alerts.Commands.GetAlertDetails;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace IntegrationTests.AlertTests;

public class GetAlertsTestsBase : BaseTestFixture
{
    public static SimpleAlertView[] XilapaAlerts { get; set; } = null!;
    public static SimpleAlertView[] XulipaAlerts { get; set; } = null!;
    public static DateTime StartingTime { get; } = new(2020, 1, 1, 0, 0, 0);

    public override Action<CustomWebApplicationOptions> Options =>
        opts => opts.DatabaseType = DatabaseType.SqliteOnDisk;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        XilapaAlerts = new SimpleAlertView[60];
        XulipaAlerts = new SimpleAlertView[60];
        AppFactory.CurrentTime = StartingTime;
        for (var i = 0; i < 60; i++)
        {
            var watchMode = i > 9 ? EWatchMode.Term : EWatchMode.AnyChanges;
            AppFactory.CurrentTime = AppFactory.CurrentTime.AddMinutes(5);

            // xilapa'll have odd alert ids
            var alertXilapa = await AppFactory
                .CreateAlert<SimpleAlertView>($"alert{(2 * (i + 1)) - 1}", watchMode, Users.Xilapa.Id, AppFactory.CurrentTime);
            XilapaAlerts[i] = alertXilapa;

            // xulipa'll have even alert ids
            var alertXulipa = await AppFactory
                .CreateAlert<SimpleAlertView>($"alert{2 * (i + 1)}", watchMode, Users.Xulipa.Id, AppFactory.CurrentTime);
            XulipaAlerts[i] = alertXulipa;
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
                    .Encode((2 * 10) - 1)
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
                    .Encode((2 * 60) - 1) // The id of the last xilapa's alert
            },
            GetAlertsTestsBase.StartingTime.AddMinutes(60 * 5), // The creation date of the last xilapa's alert
            null,
            null,
            0
        };
    }

    [Theory]
    [MemberData(nameof(PaginationData))]
    public async Task PaginationTests(GetUserAlertsCommand command, DateTime dateToFilter, int? firstid, int? lastId, int count)
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
            .GetTyped<WebApiResponse<PaginatedList<SimpleAlertView>>>();

        var resultList = typedResult!.Result!.Results.ToArray();
        typedResult.Result.Total.Should().Be(GetAlertsTestsBase.XilapaAlerts.Length);

        resultList.Length.Should().Be(count);
        if (count != 0)
        {
            resultList[0].Name.Should().Contain(firstid.ToString());
            resultList[command.Take-1].Name.Should().Contain(lastId.ToString());
        }

        resultList.Should().BeEquivalentTo(expected,
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
            15,
            GetAlertsTestsBase.XilapaAlerts?.Length!
        };

        yield return new object[]
        {
            new GetUserAlertsCommand
            {
                Take = 50,
                LastAlertId = null
            },
            50,
            GetAlertsTestsBase.XilapaAlerts?.Length!
        };

        yield return new object[]
        {
            new GetUserAlertsCommand
            {
                Take = 51,
                LastAlertId = null
            },
            50,
            GetAlertsTestsBase.XilapaAlerts?.Length!
        };
    }

    [Theory]
    [MemberData(nameof(TakeData))]
    public async Task CantTakeMoreThan50Results(GetUserAlertsCommand command, int count, int total)
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Act
        var result = await GetAsync("alert", command);

        // Assert
        result.HttpResponse!
            .StatusCode
            .Should().Be(HttpStatusCode.OK);

        var typedResult = result
            .GetTyped<WebApiResponse<PaginatedList<SimpleAlertView>>>();

        typedResult!.Result!.Total.Should().Be(total);
        typedResult.Result.Results.Count().Should().Be(count);
    }

    public static IEnumerable<object[]> AlertDetailsData()
    {
        yield return new object[]
        {
            new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength)
                .Encode(2 * (0 + 1)), // first xulipa alert
            2
        };

        yield return new object[]
        {
            new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength)
                .Encode(2 * (2 + 1)), // third xulipa alert
            6
        };
    }

    [Theory]
    [MemberData(nameof(AlertDetailsData))]
    public async Task GetAlertDetails(string hashedAlertId, int alertId)
    {
        // Arrange
        LoginAs(Users.Xulipa);

        var alertToMap = await AppFactory.WithDbContext(ctx =>
            ctx.Alerts
                .Include(a => a.WatchMode)
                .FirstAsync(a => a.Id == new AlertId(alertId)));

        var expected = await AppFactory.WithServiceProvider(provider =>
        {
            var mapper = provider.GetRequiredService<IMapper>();
            var alertDetails = mapper.Map<AlertDetails>(alertToMap);
            return Task.FromResult(alertDetails);
        });

        // Act
        var result = await GetAsync($"alert/{hashedAlertId}/details");

        // Assert
        result.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.OK);

        result.GetTyped<WebApiResponse<AlertDetails>>()!
            .Result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task CannotGetDetailsFromAnotherUserAlert()
    {
        // Arrange
        LoginAs(Users.Xulipa);

        var xilapaAlertId = GetAlertsTestsBase.XilapaAlerts[0].Id;

        // Act
        var result = await GetAsync($"alert/{xilapaAlertId}/details");

        // Assert
        result.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.OK);

        result.GetTyped<WebApiResponse<AlertDetails>>()!
            .Result.Should().BeNull();
    }
}