﻿using MockQueryable.NSubstitute;
using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Domain.Users;

namespace UnitTests.Commands;

public sealed class SendReactivateAccountEmailCommandTests
{
    [Fact]
    public async Task CantSentReactivateAccountEmailForNonExistingUser()
    {
        // Arrange
        var dbsetMock = Array.Empty<User>().AsQueryable().BuildMockDbSet();
        var contextMock = new Mock<ISiteWatcherContext>();
        contextMock.Setup(c => c.Users).Returns(dbsetMock);
        var commandHandler = new SendReactivateAccountEmailCommandHandler(contextMock.Object, null!);

        // Act
        await commandHandler.Handle(new SendReactivateAccountEmailCommand(), default);

        // Assert
        contextMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}