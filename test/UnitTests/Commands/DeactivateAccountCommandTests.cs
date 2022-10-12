using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.DeactivateAccount;

namespace UnitTests.Commands;

public sealed class DeactivateAccountCommandTests
{
        [Fact]
        public async Task CantDeactivateNonExistingUser()
        {
            // Arrange
            var userRepository = new Mock<IUserRepository>().Object;
            var uowMock = new Mock<IUnitOfWork>();
            var commandHandler = new DeactivateAccountCommandHandler(userRepository, uowMock.Object, null!);

            // Act
            await commandHandler.Handle(new DeactivateAccountCommand(), default);

            // Assert
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
}