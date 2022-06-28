using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.ConfirmEmail;
using SiteWatcher.Application.Users.Commands.ReactivateAccount;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Common;

namespace UnitTests.Commands;

public class ReactivateAccountCommandTests
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

        // Act
        var result =
            await commandHandler.Handle(new ReactivateAccountCommand {Token = "token"}, CancellationToken.None);

        // Assert
        result.Success
            .Should().BeFalse();

        result.Errors
            .Count().Should().Be(1);

        result.Errors.First()
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ReactivateAccountCommand.Token)));
    }

    [Fact]
    public async Task UserCantConfirmEmailWithInvalidToken()
    {
        // Arrange
        var user = new User("googleId", "name", "email", "authEmail",
            ELanguage.BrazilianPortuguese, ETheme.Light, DateTime.Now);
        user.Deactivate(DateTime.Now);

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(u => u.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var session = new Mock<ISession>().Object;
        var uow = new Mock<IUnitOfWork>().Object;
        var commandHandler = new ReactivateAccountCommandHandler(_authService, userRepository.Object, session, uow);

        // Act
        var result =
            await commandHandler.Handle(new ReactivateAccountCommand {Token = "INVALID_TOKEN"}, CancellationToken.None);

        // Assert
        result.Success
            .Should().BeFalse();

        result.Errors
            .Count().Should().Be(1);

        result.Errors.First()
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ReactivateAccountCommand.Token)));

        user.Active.Should().BeFalse();
        user.SecurityStamp.Should().BeNull();
    }
}