using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Domain.Enums;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace IntegrationTests.AlertTests;

public class DeleteAlertTestsBase : BaseTestFixture
{
    public override Action<CustomWebApplicationOptions> Options =>
        opts => opts.DatabaseType = DatabaseType.SqliteOnDisk;
}

public class DeleteAlertTests : BaseTest, IClassFixture<DeleteAlertTestsBase>
{
    public DeleteAlertTests(DeleteAlertTestsBase fixture) : base(fixture)
    { }

    [Fact]
    public async Task AlertIsDeletedWithValidId()
    {
        // Arrange
        const string alertName = "testAlert";
        var alertCreated = await AppFactory
            .CreateAlert<SimpleAlertView>(alertName, EWatchMode.AnyChanges, Users.Xulipa.Id, CurrentTime);

        (await AlertExists(alertName)).Should().BeTrue();

        // Act
        LoginAs(Users.Xulipa);
        var result = await DeleteAsync($"alert/{alertCreated.Id}");

        // await the fire and forget run for alerts changed event
        await Task.Delay(300);

        result.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.OK);

        result.GetTyped<WebApiResponse<object>>()!
            .Result.Should().BeNull();

        (await AlertExists(alertName)).Should().BeFalse();
    }

    [Fact]
    public async Task CantDeleteAnotherUserAlert()
    {
        // Arrange
        const string alertName = "testAlertNotDeleted";
        var alertCreated = await AppFactory
            .CreateAlert<SimpleAlertView>(alertName, EWatchMode.AnyChanges, Users.Xulipa.Id, CurrentTime);

        (await AlertExists(alertName)).Should().BeTrue();

        // Act
        LoginAs(Users.Xilapa);
        var result = await DeleteAsync($"alert/{alertCreated.Id}");

        result.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        result.GetTyped<WebApiResponse<object>>()!
            .Result.Should().BeNull();

        (await AlertExists(alertName)).Should().BeTrue();
    }

    private async Task<bool> AlertExists(string alertName) =>
        await AppFactory.WithDbContext(ctx =>
            ctx.Alerts.AnyAsync(a => a.Name == alertName));
}