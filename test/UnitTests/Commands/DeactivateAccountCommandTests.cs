using MockQueryable.NSubstitute;
using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.DeactivateAccount;
using SiteWatcher.Domain.Users;

namespace UnitTests.Commands;

public sealed class DeactivateAccountCommandTests
{
    [Fact]
    public async Task CantDeactivateNonExistingUser()
    {
        // Arrange
        var dbSetMock = Array.Empty<User>().AsQueryable().BuildMockDbSet();
        var contextMock = new Mock<ISiteWatcherContext>();
        contextMock.Setup(c => c.Users).Returns(dbSetMock);
        var commandHandler = new DeactivateAccountCommandHandler(contextMock.Object, null!);

        // Act
        await commandHandler.Handle(new DeactivateAccountCommand(), default);

        // Assert
        contextMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}