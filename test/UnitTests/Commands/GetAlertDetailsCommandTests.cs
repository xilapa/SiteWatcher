using FluentAssertions;
using Moq;
using SiteWatcher.Application.Alerts.Commands.GetAlertDetails;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace UnitTests.Commands;

public class GetAlertDetailsCommandTests
{
    [Fact]
    public async Task RepositoryIsNotCalledWithInvalidHash()
    {
        // Arrange
        var alertRepoMock = new Mock<IAlertDapperRepository>();
        var idHasher = new IdHasher(new TestAppSettings());
        var handler = new GetAlerDetailsCommandHandler(null!, alertRepoMock.Object, idHasher, null!);

        var command = new GetAlertDetailsCommand {AlertId = "invalid"};

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Value.Should().BeNull();

        alertRepoMock
            .Verify(r => r.GetAlertDetails(It.IsAny<int>(), It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}