using FluentAssertions;
using Moq;
using SiteWatcher.Application.Alerts.Commands.GetAlertDetails;
using SiteWatcher.Domain.Alerts.Repositories;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace UnitTests.Commands;

public sealed class GetAlertDetailsCommandTests
{
    [Fact]
    public async Task RepositoryIsNotCalledWithInvalidHash()
    {
        // Arrange
        var alertRepoMock = new Mock<IAlertDapperRepository>();
        var idHasher = new IdHasher(new TestAppSettings());
        var handler = new GetAlertDetailsCommandHandler(null!, alertRepoMock.Object, idHasher);

        var command = new GetAlertDetailsCommand {AlertId = "invalid"};

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.Should().BeNull();

        alertRepoMock
            .Verify(r => r.GetAlertDetails(It.IsAny<int>(), It.IsAny<UserId>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}