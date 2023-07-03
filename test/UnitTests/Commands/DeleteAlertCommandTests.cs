using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SiteWatcher.Application.Alerts.Commands.DeleteAlert;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace UnitTests.Commands;

public sealed class DeleteAlertCommandTests
{
    [Fact]
    public async Task InvalidAlertIdDoesntDeleteAnything()
    {
        // Arrange
        var alertDbSetMock = new Mock<DbSet<Alert>>();
        var contextMock = new Mock<ISiteWatcherContext>();
        contextMock.Setup(c => c.Alerts).Returns(alertDbSetMock.Object);

        var idHasher = new IdHasher(new TestAppSettings());
        var handler = new DeleteAlertCommandHandler(contextMock.Object, idHasher, null!, null!);

        var command = new DeleteAlertCommand {AlertId = "invalidId"};

        // Act
        var result = await handler.Handle(command, default) as ErrorResult;

        // Assert
        result!.Errors.Count().Should().Be(1);
        result.Errors.First().Should().Be(ApplicationErrors.ValueIsInvalid(nameof(DeleteAlertCommand.AlertId)));
    }
}