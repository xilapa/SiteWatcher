﻿using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Alerts.Commands.DeleteAlert;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AlertTests;

public sealed class DeleteAlertTestsBase : BaseTestFixture
{
    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseDatabase(DatabaseType.SqliteOnDisk);
    }
}

public sealed class DeleteAlertTests : BaseTest, IClassFixture<DeleteAlertTestsBase>
{
    public DeleteAlertTests(DeleteAlertTestsBase fixture) : base(fixture)
    { }

    [Fact]
    public async Task AlertIsDeletedWithValidId()
    {
        // Arrange
        const string alertName = "testAlert";
        var alertCreated = await AppFactory
            .CreateAlert<SimpleAlertView>(alertName, RuleType.AnyChanges, Users.Xulipa.Id);

        (await AlertExists(alertName)).Should().BeTrue();

        // Act
        LoginAs(Users.Xulipa);
        var result = await DeleteAsync($"alert/{alertCreated.Id}");

        result.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        result.HttpMessageContent.Should().Be(string.Empty);

        (await AlertExists(alertName)).Should().BeFalse();
    }

    [Fact]
    public async Task CantDeleteAnotherUserAlert()
    {
        // Arrange
        const string alertName = "testAlertNotDeleted";
        var alertCreated = await AppFactory
            .CreateAlert<SimpleAlertView>(alertName, RuleType.AnyChanges, Users.Xulipa.Id);

        (await AlertExists(alertName)).Should().BeTrue();

        // Act
        LoginAs(Users.Xilapa);
        var result = await DeleteAsync($"alert/{alertCreated.Id}");

        result.HttpResponse!
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        result.GetTyped<string[]>()
            .Should().BeEquivalentTo(ApplicationErrors.ValueIsInvalid(nameof(DeleteAlertCommand.AlertId)));

            (await AlertExists(alertName)).Should().BeTrue();
    }

    private async Task<bool> AlertExists(string alertName) =>
        await AppFactory.WithDbContext(ctx =>
            ctx.Alerts.AnyAsync(a => a.Name == alertName));
}