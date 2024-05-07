using MockQueryable.NSubstitute;
using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.SendEmailConfirmation;
using SiteWatcher.Domain.Users;

namespace UnitTests.Commands;

public sealed class SendEmailConfirmationCommandTests
{
    [Fact]
    public async Task CantSentEmailConfirmationForNonExistingUser()
    {
        // Arrange
        var dbsetMock = Array.Empty<User>().AsQueryable().BuildMockDbSet();
        var contextMock = new Mock<ISiteWatcherContext>();
        contextMock.Setup(c => c.Users).Returns(dbsetMock);
        var commandHandler = new SendEmailConfirmationCommandHandler(null!, contextMock.Object);

        // Act
        await commandHandler.Handle(new SendEmailConfirmationCommand(), default);

        // Assert
        contextMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}