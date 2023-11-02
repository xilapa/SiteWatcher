using System.Net;
using FluentAssertions;
using HashidsNet;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AlertTests;

public sealed class GetAlertsTestsBase : BaseTestFixture
{
    public static SimpleAlertView[] XilapaAlerts { get; set; } = null!;
    public static SimpleAlertView[] XulipaAlerts { get; set; } = null!;
    public static DateTime StartingTime { get; } = new(2020, 1, 1, 0, 0, 0);

    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseDatabase(DatabaseType.SqliteOnDisk);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        XilapaAlerts = new SimpleAlertView[60];
        XulipaAlerts = new SimpleAlertView[60];
        AppFactory.CurrentTime = StartingTime;
        for (var i = 0; i < 60; i++)
        {
            var rule = i > 9 ? RuleType.Term : RuleType.AnyChanges;
            AppFactory.CurrentTime = AppFactory.CurrentTime.AddMinutes(5);

            // xilapa'll have odd alert ids
            var alertXilapa = await AppFactory
                .CreateAlert<SimpleAlertView>($"alert{(2 * (i + 1)) - 1}", rule, Users.Xilapa.Id);
            XilapaAlerts[i] = alertXilapa;

            // xulipa'll have even alert ids
            var alertXulipa = await AppFactory
                .CreateAlert<SimpleAlertView>($"alert{2 * (i + 1)}", rule, Users.Xulipa.Id);
            XulipaAlerts[i] = alertXulipa;
        }
    }
}

public sealed class GetAlertsTests : BaseTest, IClassFixture<GetAlertsTestsBase>
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

        result.GetTyped<PaginatedList<SimpleAlertView>>()!
            .Results.Should()
            .BeEquivalentTo(GetAlertsTestsBase.XilapaAlerts.Take(10),
                opt => opt.WithoutStrictOrdering());
    }

    public static IEnumerable<object[]> PaginationData()
    {
        yield return new object[]
        {
            new GetUserAlertsQuery
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
            new GetUserAlertsQuery
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
            new GetUserAlertsQuery
            {
                Take = 10,
                LastAlertId = new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength)
                    .Encode((2 * 60) - 1) // The id of the last xilapa's alert
            },
            GetAlertsTestsBase.StartingTime.AddMinutes(60 * 5), // The creation date of the last xilapa's alert
            null!,
            null!,
            0
        };
    }

    [Theory]
    [MemberData(nameof(PaginationData))]
    public async Task PaginationTests(GetUserAlertsQuery query, DateTime dateToFilter, int? firstid, int? lastId, int count)
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Act
        var result = await GetAsync("alert", query);

        // Assert
        result.HttpResponse!
            .StatusCode
            .Should().Be(HttpStatusCode.OK);

        query.Take = query.Take == 0 ? 10 : query.Take;

        var expected = GetAlertsTestsBase.XilapaAlerts
            .Where(a => a.CreatedAt > dateToFilter)
            .Take(query.Take)
            .ToArray();

        var typedResult = result.GetTyped<PaginatedList<SimpleAlertView>>();

        var resultList = typedResult!.Results.ToArray();
        typedResult.Total.Should().Be(GetAlertsTestsBase.XilapaAlerts.Length);

        resultList.Length.Should().Be(count);
        if (count != 0)
        {
            resultList[0].Name.Should().Contain(firstid.ToString());
            resultList[query.Take-1].Name.Should().Contain(lastId.ToString());
        }

        resultList.Should().BeEquivalentTo(expected,
            opt => opt.WithStrictOrdering());
    }

    public static IEnumerable<object[]> TakeData()
    {
        yield return new object[]
        {
            new GetUserAlertsQuery
            {
                Take = 15,
                LastAlertId = null
            },
            15,
            GetAlertsTestsBase.XilapaAlerts?.Length!
        };

        yield return new object[]
        {
            new GetUserAlertsQuery
            {
                Take = 50,
                LastAlertId = null
            },
            50,
            GetAlertsTestsBase.XilapaAlerts?.Length!
        };

        yield return new object[]
        {
            new GetUserAlertsQuery
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
    public async Task CantTakeMoreThan50Results(GetUserAlertsQuery query, int count, int total)
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Act
        var result = await GetAsync("alert", query);

        // Assert
        result.HttpResponse!
            .StatusCode
            .Should().Be(HttpStatusCode.OK);

        var typedResult = result.GetTyped<PaginatedList<SimpleAlertView>>();

        typedResult!.Total.Should().Be(total);
        typedResult.Results.Length.Should().Be(count);
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
                .Include(a => a.Rule)
                .FirstAsync(a => a.Id == new AlertId(alertId)));

        var expected = await AppFactory.WithServiceProvider(provider =>
        {
            var idHasher = provider.GetRequiredService<IIdHasher>();
            return Task.FromResult(AlertDetails.FromAlert(alertToMap, idHasher));
        });

        // Act
        var result = await GetAsync($"alert/{hashedAlertId}/details");

        // Assert
        result.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.OK);

        result.GetTyped<AlertDetails>()!
            .Should().BeEquivalentTo(expected);
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
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        result.HttpMessageContent.Should().BeEquivalentTo(string.Empty);
    }
}