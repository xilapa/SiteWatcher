using FluentAssertions;
using Moq;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;

namespace UnitTests.Commands;

public sealed class GetUserAlertsCommandTests
{
    [Fact]
    public async Task RepositoryIsNotCalledWithTakeEqualsToZero()
    {
        // Arrange
        var command = new GetUserAlertsCommand {Take = 0};
        var alertDapperRepoMock = new Mock<IAlertDapperRepository>();
        var handler = new GetUserAlertsCommandHandler(null!, null!, alertDapperRepoMock.Object, null!);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().BeAssignableTo<EmptyResult>();
        alertDapperRepoMock
            .Verify(r =>
                r.GetUserAlerts(It.IsAny<UserId>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}