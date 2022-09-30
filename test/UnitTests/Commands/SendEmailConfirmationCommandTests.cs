using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.SendEmailConfirmation;

namespace UnitTests.Commands;

public sealed class SendEmailConfirmationCommandTests
{
    [Fact]
    public async Task CantSentEmailConfirmationForNonExistingUser()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>().Object;
        var uowMock = new Mock<IUnitOfWork>();
        var commandHandler = new SendEmailConfirmationCommandHandler(null!, userRepository, uowMock.Object);

        // Act
        await commandHandler.Handle(new SendEmailConfirmationCommand(), default);

        // Assert
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}