﻿using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.ReactivateAccount;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;

namespace UnitTests.Commands;

public sealed class ReactivateAccountCommandTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    [Fact]
    public async Task CantReactiveAccountIfUserDoesNotExists()
    {
        // Arrange
        var userRepository = Array.Empty<User>().AsQueryable().BuildMockDbSet();
        var mockContext = new Mock<ISiteWatcherContext>();
        mockContext.Setup(c => c.Users).Returns(userRepository.Object);

        _authServiceMock
            .Setup(a => a.GetUserIdFromConfirmationToken(It.IsAny<string>()))
            .ReturnsAsync(new UserId(Guid.NewGuid()));

        var commandHandler = new ReactivateAccountCommandHandler(_authServiceMock.Object, mockContext.Object, null!, null!);
        var command = new ReactivateAccountCommand {Token = "token"};

        // Act
        var result = await commandHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Error!.Messages
            .Length.Should().Be(1);

        result.Error.Messages[0]
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ReactivateAccountCommand.Token)));
    }

    [Fact]
    public async Task UserCantReactivateAccountlWithInvalidToken()
    {
        // Arrange
        var registerInput = new RegisterUserInput("name", "email", Language.BrazilianPortuguese, Theme.Light,
            "googleId", "authEmail");
        var (user, _) = User.Create(registerInput, DateTime.Now);
        user.Deactivate(DateTime.Now);

        var userDbSetMock = new[] { user }.AsQueryable().BuildMockDbSet();
        var contextMock = new Mock<ISiteWatcherContext>();
        contextMock.Setup(c => c.Users).Returns(userDbSetMock.Object);

        _authServiceMock
            .Setup(a => a.GetUserIdFromConfirmationToken(It.IsAny<string>()))
            .ReturnsAsync(user.Id);

        var session = new Mock<ISession>().Object;
        var commandHandler = new ReactivateAccountCommandHandler(_authServiceMock.Object, contextMock.Object, session, null!);
        var command = new ReactivateAccountCommand {Token = "INVALID_TOKEN"};

        // Act
        var result = await commandHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Error!.Messages
            .Length.Should().Be(1);

        result.Error.Messages[0]
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ReactivateAccountCommand.Token)));

        user.Active.Should().BeFalse();
        user.SecurityStamp.Should().BeNull();
    }
}