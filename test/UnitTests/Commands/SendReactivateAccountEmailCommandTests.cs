using Moq;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Users.Repositories;

namespace UnitTests.Commands;

public sealed class SendReactivateAccountEmailCommandTests
{
    [Fact]
    public async Task CantSentReactivateAccountEmailForNonExistingUser()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>().Object;
        var uowMock = new Mock<IUnitOfWork>();
        var commandHandler = new SendReactivateAccountEmailCommandHandler(userRepository, null! , uowMock.Object);

        // Act
        await commandHandler.Handle(new SendReactivateAccountEmailCommand(), default);

        // Assert
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}