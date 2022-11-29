using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Repositories;

namespace UnitTests.Commands;

public sealed class UpdateUserCommandTests
{
    [Fact]
    public async Task CantUpdateNonExistingUser()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(u => u.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<User?>(default));
        var uow = new Mock<IUnitOfWork>().Object;
        var authService = new Mock<IAuthService>().Object;
        var session = new Mock<ISession>().Object;

        var commandHandler = new UpdateUserCommandHandler(userRepositoryMock.Object, uow, authService, session);

        // Act
        var result = await commandHandler.Handle(new UpdateUserCommand(), CancellationToken.None) as ErrorResult;

        // Assert

        result!.Errors
            .Count().Should().Be(1);

        result.Errors.First()
            .Should().Be(ApplicationErrors.USER_DO_NOT_EXIST);
    }
}