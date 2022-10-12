using System.Net;
using Domain.DTOs.Alerts;
using FluentAssertions;
using HashidsNet;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AlertTests;

public sealed class CreateAlertTests : BaseTest, IClassFixture<BaseTestFixture>
{
    public CreateAlertTests(BaseTestFixture fixture) : base(fixture)
    { }

    public static IEnumerable<object[]> CreateAlertData()
    {
        yield return new object[]
        {
            new CreateAlertCommand(),
            HttpStatusCode.BadRequest,
            null!, // DetailedAlertView
            new[]
            {
                ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.Name)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)),
                ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.SiteName)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.SiteUri)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.WatchMode))
            }
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
            new DetailedAlertView
                {
                    Id = new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength).Encode(1),
                    Name = "Test Alert1",
                    Frequency = EFrequency.TwentyFourHours,
                    Site = new SiteView {Name="store site", Uri="https://store.site.io/"},
                    WatchMode = new DetailedWatchModeView
                    {
                        WatchMode = EWatchMode.AnyChanges
                    }
                },
            null! // errors
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
            new DetailedAlertView
                {
                    Id = new Hashids(TestAppSettings.TestHashIdSalt, TestAppSettings.TestHashedIdLength).Encode(2),
                    Name = "Test Alert2",
                    Frequency = EFrequency.TwentyFourHours,
                    Site = new SiteView {Name="store site", Uri="https://store.site.io/"},
                    WatchMode = new DetailedWatchModeView
                    {
                        WatchMode = EWatchMode.Term,
                        Term = "lookup term"
                    }
                },
            null! // errors
        };
    }

    [Theory]
    [MemberData(nameof(CreateAlertData))]
    public async Task AlertWithAnyChangesWatchIsCreatedCorrectly(CreateAlertCommand command,
        HttpStatusCode expectedStatusCode, DetailedAlertView? expectedResult, string[]? errors)
    {
        // Arrange
        LoginAs(Users.Xilapa);
        if(expectedResult is not null)
            expectedResult.CreatedAt = CurrentTime;

        // Act
        var result = await PostAsync("alert", command);

        // Assert
        result.HttpResponse!.StatusCode
            .Should().Be(expectedStatusCode);

        if (errors is not null)
        {
            var typedErrorResult = result.GetTyped<string[]>();
            typedErrorResult.Should().BeEquivalentTo(errors);

            (await AppFactory.WithDbContext(ctx =>
            {
                return ctx.Alerts
                    .Include(a => a.WatchMode)
                    .FirstOrDefaultAsync(a => a.Name == command.Name);
            })).Should().BeNull();

            return;
        }

        var typedResult = result.GetTyped<DetailedAlertView>();
        typedResult.Should().BeEquivalentTo(expectedResult);

        // Check on database
        if (!HttpStatusCode.OK.Equals(expectedStatusCode))
            return;

        var alertFromDatabase = await AppFactory.WithDbContext(ctx =>
        {
            return ctx.Alerts
                .Include(a => a.WatchMode)
                .FirstOrDefaultAsync(a => a.Name == command.Name);
        });

        var alertFromDbMapped = await AppFactory.WithServiceProvider(prv =>
        {
            var idHasher = prv.GetRequiredService<IIdHasher>();
            return Task.FromResult(alertFromDatabase!.ToDetailedAlertView(idHasher));
        });

        typedResult!.Should().BeEquivalentTo(alertFromDbMapped);
    }
}