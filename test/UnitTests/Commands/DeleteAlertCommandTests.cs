using FluentAssertions;
using Moq;
using SiteWatcher.Application.Alerts.Commands.DeleteAlert;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Alerts.Repositories;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace UnitTests.Commands;

public sealed class DeleteAlertCommandTests
{
    [Fact]
    public async Task InvalidAlertIdDoesntCallTheDatabase()
    {
        // Arrange
        var alertDapperRepoMock = new Mock<IAlertDapperRepository>();
        var idHasher = new IdHasher(new TestAppSettings());
        var handler = new DeleteAlertCommandHandler(alertDapperRepoMock.Object, idHasher, null!, null!);

        var command = new DeleteAlertCommand {AlertId = "invalidId"};

        // Act
        var result = await handler.Handle(command, default) as ErrorResult;

        // Assert
        result!.Errors.Count().Should().Be(1);
        result.Errors.First().Should().Be(ApplicationErrors.ValueIsInvalid(nameof(DeleteAlertCommand.AlertId)));
        alertDapperRepoMock
            .Verify(r =>
                r.DeleteUserAlert(It.IsAny<int>(), It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}