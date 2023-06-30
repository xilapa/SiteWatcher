using System.Data;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;

namespace UnitTests.Commands;

public sealed class GetUserAlertsCommandTests
{
    [Fact]
    public async Task RepositoryIsNotCalledWithTakeEqualsToZero()
    {
        // Arrange
        var command = new GetUserAlertsCommand {Take = 0};
        var dapperContextMock = new Mock<IDapperContext>();
        var handler = new GetUserAlertsCommandHandler(null!, null!, dapperContextMock.Object, null!);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().BeAssignableTo<EmptyResult>();
        dapperContextMock
            .Verify(r =>
                r.UsingConnectionAsync(It.IsAny<Func<IDbConnection,Task<It.IsAnyType>>>()),
                Times.Never);
    }
}