﻿using System.Net;
using AutoMapper;
using Domain.DTOs.Alert;
using FluentAssertions;
using HashidsNet;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Enums;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace IntegrationTests.AlertTests;

public class CreateAlertTests : BaseTest, IClassFixture<BaseTestFixture>
{
    public CreateAlertTests(BaseTestFixture fixture) : base(fixture)
    { }

    public static IEnumerable<object[]> CreateAlertData()
    {
        yield return new object[]
        {
            new CreateAlertCommand(),
            HttpStatusCode.BadRequest,
            new WebApiResponse<DetailedAlertView>()
                .AddMessages(
                    ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.Name)),
                    ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)),
                    ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.SiteName)),
                    ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.SiteUri)),
                    ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.WatchMode))
                    )
        };

        yield return new object[]
        {
            new CreateAlertCommand
            {
                Name = "Test Alert1",
                Frequency = EFrequency.TwentyFourHours,
                SiteName = "store site",
                SiteUri = "https://store.site.io",
                WatchMode = EWatchMode.AnyChanges
            },
            HttpStatusCode.OK,
            new WebApiResponse<DetailedAlertView>()
                .SetResult(new DetailedAlertView
                {
                    Id = new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength).Encode(1),
                    Name = "Test Alert1",
                    Frequency = EFrequency.TwentyFourHours,
                    Site = new SiteView {Name="store site", Uri="https://store.site.io/"},
                    WatchMode = new DetailedWatchModeView
                    {
                        Id = new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength).Encode(1),
                        WatchMode = EWatchMode.AnyChanges
                    }
                })
        };

        yield return new object[]
        {
            new CreateAlertCommand
            {
                Name = "Test Alert2",
                Frequency = EFrequency.TwentyFourHours,
                SiteName = "store site",
                SiteUri = "https://store.site.io",
                WatchMode = EWatchMode.Term,
                Term = "lookup term"
            },
            HttpStatusCode.OK,
            new WebApiResponse<DetailedAlertView>()
                .SetResult(new DetailedAlertView
                {
                    Id = new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength).Encode(2),
                    Name = "Test Alert2",
                    Frequency = EFrequency.TwentyFourHours,
                    Site = new SiteView {Name="store site", Uri="https://store.site.io/"},
                    WatchMode = new DetailedWatchModeView
                    {
                        Id = new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength).Encode(2),
                        WatchMode = EWatchMode.Term,
                        Term = "lookup term"
                    }
                })
        };
    }

    [Theory]
    [MemberData(nameof(CreateAlertData))]
    public async Task AlertWithAnyChangesWatchIsCreatedCorrectly(CreateAlertCommand command,
        HttpStatusCode expectedStatusCode, WebApiResponse<DetailedAlertView> expectedResult)
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Act
        var result = await PostAsync("alert", command);

        // Assert
        result.HttpResponse!.StatusCode
            .Should().Be(expectedStatusCode);

        var typedResult = result.GetTyped<WebApiResponse<DetailedAlertView>>();
        typedResult.Should().BeEquivalentTo(expectedResult);

        // Check on database
        if (!HttpStatusCode.OK.Equals(expectedStatusCode))
            return;

        var alertFromDatabase = await WithDbContext(ctx =>
        {
            return ctx.Alerts
                .Include(a => a.WatchMode)
                .FirstOrDefaultAsync(a => a.Name == command.Name);
        });

        var alertFromDbMapped = await WithServiceProvider(prv =>
        {
            var mapper = prv.GetRequiredService<IMapper>();
            var alertView = mapper.Map<DetailedAlertView>(alertFromDatabase);
            return Task.FromResult(alertView);
        });

        typedResult!.Result.Should().BeEquivalentTo(alertFromDbMapped);
    }
}