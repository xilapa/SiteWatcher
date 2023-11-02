using System.Data;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Interfaces;

namespace UnitTests.Commands;

public sealed class GetUserAlertsCommandTests
{
    [Fact]
    public async Task RepositoryIsNotCalledWithTakeEqualsToZero()
    {
        // Arrange
        var command = new GetUserAlertsQuery {Take = 0};
        var dapperContextMock = new Mock<IDapperContext>();
        var handler = new GetUserAlertsQueryHandler(null!, null!, dapperContextMock.Object, null!);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Value.Should().BeNull();
        dapperContextMock
            .Verify(r =>
                r.UsingConnectionAsync(It.IsAny<Func<IDbConnection,Task<It.IsAnyType>>>()),
                Times.Never);
    }
}