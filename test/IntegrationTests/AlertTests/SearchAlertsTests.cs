﻿using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using SiteWatcher.Application.Alerts.Commands.SearchAlerts;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AlertTests;

public sealed class SearchAlertsTestsBase : BaseTestFixture
{
    public static SimpleAlertView XilapaWhiteShirt;
    public static SimpleAlertView XilapaBlueShirt;
    public static SimpleAlertView XilapaSmartphone;
    public static SimpleAlertView XilapaBlueMousepad;
    public static SimpleAlertView XulipaWhiteCap;
    public static SimpleAlertView XulipaBlueShorts;
    public static SimpleAlertView XulipaAletorio;

    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseDatabase(DatabaseType.Postgres);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        #region Xilapa alerts

        XilapaWhiteShirt = await AppFactory.CreateAlert<SimpleAlertView>("white shirt", RuleType.AnyChanges,
            Users.Xilapa.Id, new DateTime(2020,09,15,18, 32,43,DateTimeKind.Utc),
            "cloth store", "http://clothstore.com");
        XilapaBlueShirt = await AppFactory.CreateAlert<SimpleAlertView>("blue shirt", RuleType.AnyChanges,
            Users.Xilapa.Id, new DateTime(2021,10,15,18, 31,55,DateTimeKind.Utc),
            "cloth store", "http://clothstore.com");
        XilapaSmartphone = await AppFactory.CreateAlert<SimpleAlertView>("smartphone", RuleType.AnyChanges,
            Users.Xilapa.Id, new DateTime(2021,10,15,18, 31,55,DateTimeKind.Utc),
            "eletroshop", "http://eletroshop.com");
        XilapaBlueMousepad = await AppFactory.CreateAlert<SimpleAlertView>("blue mousepad", RuleType.AnyChanges,
            Users.Xilapa.Id, new DateTime(2021,10,16,18, 31,55,DateTimeKind.Utc),
            "cheap things", "http://cheapthings.com");

        #endregion

        #region Xulipa alerts

        XulipaWhiteCap = await AppFactory.CreateAlert<SimpleAlertView>("white cap", RuleType.AnyChanges,
            Users.Xulipa.Id, new DateTime(2020,09,15,19, 32,43,DateTimeKind.Utc),
            "cloth store", "http://clothstore.com");
        XulipaBlueShorts = await AppFactory.CreateAlert<SimpleAlertView>("blue shorts", RuleType.AnyChanges,
            Users.Xulipa.Id, new DateTime(2021,10,17,18, 31,55,DateTimeKind.Utc),
            "the bazar", "http://thebazar.com");
        XulipaAletorio = await AppFactory.CreateAlert<SimpleAlertView>("carroça do vigário", RuleType.AnyChanges,
            Users.Xulipa.Id, new DateTime(2021,10,18,18, 31,55,DateTimeKind.Utc),
            "site aleatório", "http://umlinkdiferente.com");

        #endregion
    }
}

public sealed class SearchAlertsTests : BaseTest, IClassFixture<SearchAlertsTestsBase>
{
    public SearchAlertsTests(SearchAlertsTestsBase fixture) : base(fixture)
    { }

    public static IEnumerable<object[]> SearchResults()
    {
        yield return new object[]
        {
            "shirt",
            Users.Xilapa,
            new []
            {
                SearchAlertsTestsBase.XilapaBlueShirt,
                SearchAlertsTestsBase.XilapaWhiteShirt
            }
        };

        yield return new object[]
        {
            "blue shirt",
            Users.Xilapa,
            new []
            {
                SearchAlertsTestsBase.XilapaBlueShirt,
                SearchAlertsTestsBase.XilapaWhiteShirt,
                SearchAlertsTestsBase.XilapaBlueMousepad
            }
        };

        yield return new object[]
        {
            "the bazar",
            Users.Xulipa,
            new []
            {
                SearchAlertsTestsBase.XulipaBlueShorts
            }
        };

        yield return new object[]
        {
            "Carroca VIGario",
            Users.Xulipa,
            new []
            {
                SearchAlertsTestsBase.XulipaAletorio
            }
        };

        yield return new object[]
        {
            "ÇarrÓca VIGÁríó",
            Users.Xulipa,
            new []
            {
                SearchAlertsTestsBase.XulipaAletorio
            }
        };

        yield return new object[]
        {
            "feren",
            Users.Xulipa,
            new []
            {
                SearchAlertsTestsBase.XulipaAletorio
            }
        };

        yield return new object[]
        {
            "nonexistent",
            Users.Xulipa,
            Array.Empty<SimpleAlertView>()
        };
    }

    [Theory]
    [MemberData(nameof(SearchResults))]
    public async Task SearchWorks(string searchTerm, UserViewModel loggedUser, SimpleAlertView[]? expectedSearchResults)
    {
        // Arrange
        LoginAs(loggedUser);
        var searchCommand = new SearchAlertQuery {Term = searchTerm};

        // Act
        var httpResult = await GetAsync("alert/search", searchCommand);

        // Assert
        httpResult.HttpResponse!.StatusCode
            .Should().Be(HttpStatusCode.OK);

        var searchResult = httpResult.GetTyped<IEnumerable<SimpleAlertView>>();

        searchResult.Should().BeEquivalentTo(expectedSearchResults,
            opt => opt.WithStrictOrdering());
    }
}