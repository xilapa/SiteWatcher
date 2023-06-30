using System.Data;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Alerts.Commands.GetAlertDetails;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace UnitTests.Commands;

public sealed class GetAlertDetailsCommandTests
{
    [Fact]
    public async Task RepositoryIsNotCalledWithInvalidHash()
    {
        // Arrange
        var dapperContextMock = new Mock<IDapperContext>();
        var idHasher = new IdHasher(new TestAppSettings());
        var handler = new GetAlertDetailsCommandHandler(null!, dapperContextMock.Object, null!, idHasher);

        var command = new GetAlertDetailsCommand {AlertId = "invalid"};

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().BeNull();

        dapperContextMock
            .Verify(r => r.UsingConnectionAsync(It.IsAny<Func<IDbConnection,Task<It.IsAnyType>>>()),
                Times.Never);
    }
}