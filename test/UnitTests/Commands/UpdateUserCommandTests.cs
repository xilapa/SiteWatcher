using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MockQueryable.Moq;
using Moq;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Users;

namespace UnitTests.Commands;

public sealed class UpdateUserCommandTests
{
    [Fact]
    public async Task CantUpdateNonExistingUser()
    {
        // Arrange
        var dbSetMock = Array.Empty<User>().AsQueryable().BuildMockDbSet();
        var contextMock = new Mock<ISiteWatcherContext>();
        contextMock.Setup(c => c.Users).Returns(dbSetMock.Object);
        var session = new Mock<ISession>().Object;

        var validatorMock = new Mock<IValidator<UpdateUserCommand>>();
        validatorMock.Setup(v => v.Validate(It.IsAny<UpdateUserCommand>()))
            .Returns(new ValidationResult());

        var commandHandler =
            new UpdateUserCommandHandler(contextMock.Object, session, validatorMock.Object, null!, null!);

        // Act
        var result = await commandHandler.Handle(new UpdateUserCommand(), CancellationToken.None);

        // Assert

        result.Error!.Messages.Length.Should().Be(1);
        result.Error.Messages[0]
            .Should()
            .Be(ApplicationErrors.USER_DO_NOT_EXIST);
    }
}