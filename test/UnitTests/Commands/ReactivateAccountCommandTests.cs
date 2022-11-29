using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.ReactivateAccount;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Users.Repositories;

namespace UnitTests.Commands;

public sealed class ReactivateAccountCommandTests
{
    private readonly IAuthService _authService;

    public ReactivateAccountCommandTests()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(a => a.GetUserIdFromConfirmationToken(It.IsAny<string>()))
            .ReturnsAsync(new UserId());
        _authService = authService.Object;
    }

    [Fact]
    public async Task CantReactiveAccountIfUserDoesNotExists()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(u => u.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<User?>(default));

        var commandHandler = new ReactivateAccountCommandHandler(_authService, userRepository.Object, null!, null!);
        var command = new ReactivateAccountCommand {Token = "token"};

        // Act
        var result = await commandHandler.Handle(command, CancellationToken.None) as ErrorResult;

        // Assert

        result!.Errors
            .Count().Should().Be(1);

        result.Errors.First()
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ReactivateAccountCommand.Token)));
    }

    [Fact]
    public async Task UserCantConfirmEmailWithInvalidToken()
    {
        // Arrange
        var user = new User("googleId", "name", "email", "authEmail",
            Language.BrazilianPortuguese, Theme.Light, DateTime.Now);
        user.Deactivate(DateTime.Now);

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(u => u.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var session = new Mock<ISession>().Object;
        var uow = new Mock<IUnitOfWork>().Object;
        var commandHandler = new ReactivateAccountCommandHandler(_authService, userRepository.Object, session, uow);
        var command = new ReactivateAccountCommand {Token = "INVALID_TOKEN"};

        // Act
        var result = await commandHandler.Handle(command, CancellationToken.None) as ErrorResult;

        // Assert

        result!.Errors
            .Count().Should().Be(1);

        result.Errors.First()
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ReactivateAccountCommand.Token)));

        user.Active.Should().BeFalse();
        user.SecurityStamp.Should().BeNull();
    }
}