using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.ConfirmEmail;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Users.Repositories;

namespace UnitTests.Commands;

public sealed class ConfirmEmailCommandTests
{
    private readonly IAuthService _authService;

    public ConfirmEmailCommandTests()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(a => a.GetUserIdFromConfirmationToken(It.IsAny<string>()))
            .ReturnsAsync(new UserId());
        _authService = authService.Object;
    }

    [Fact]
    public async Task CantConfirmEmailIfUserDoesNotExists()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(u => u.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<User?>(default));

        var commandHandler = new ConfirmEmailCommandHandler(_authService, userRepository.Object, null!, null!);

        // Act
        var result = await commandHandler.Handle(new ConfirmEmailCommand(), CancellationToken.None) as ErrorResult;

        // Assert
        result!.Errors.Count().Should().Be(1);
        result.Errors.First()
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ConfirmEmailCommand.Token)));
    }

    [Fact]
    public async Task UserCantConfirmEmailWithInvalidToken()
    {
        // Arrange
        var user = new User("googleId", "name", "email", "authEmail",
            Language.BrazilianPortuguese, Theme.Light, DateTime.Now);

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(u => u.GetAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var session = new Mock<ISession>().Object;
        var uow = new Mock<IUnitOfWork>().Object;
        var commandHandler = new ConfirmEmailCommandHandler(_authService, userRepository.Object, session, uow);

        // Act
        var result = await commandHandler.Handle(new ConfirmEmailCommand { Token = "INVALID_TOKEN"}, CancellationToken.None) as ErrorResult;

        // Assert

        result!.Errors
            .Count().Should().Be(1);

        result.Errors.First()
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ConfirmEmailCommand.Token)));

        user.EmailConfirmed.Should().BeFalse();
        user.SecurityStamp.Should().BeNull();
    }
}