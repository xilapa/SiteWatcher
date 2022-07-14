using FluentAssertions;
using Moq;
using SiteWatcher.Application.Alerts.Commands.DeleteAlert;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace UnitTests.Commands;

public class DeleteAlertCommandTests
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
        var result = await handler.Handle(command, default);

        // Assert
        result.Success.Should().BeFalse();
        alertDapperRepoMock
            .Verify(r =>
                r.DeleteUserAlert(It.IsAny<int>(), It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}