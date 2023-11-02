using FluentValidation;
using FluentValidation.Results;
using MockQueryable.Moq;
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
        contextMock.Setup(c => c.Users).Returns(dbsetMock.Object);

        var validatorMock = new Mock<IValidator<SendReactivateAccountEmailCommand>>();
        validatorMock.Setup(v => v.Validate(It.IsAny<SendReactivateAccountEmailCommand>()))
            .Returns(new ValidationResult());

        var commandHandler =
            new SendReactivateAccountEmailCommandHandler(contextMock.Object, null!, validatorMock.Object);

        // Act
        await commandHandler.Handle(new SendReactivateAccountEmailCommand(), default);

        // Assert
        contextMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}