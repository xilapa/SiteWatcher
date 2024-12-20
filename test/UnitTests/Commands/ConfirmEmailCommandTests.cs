﻿using FluentAssertions;
using MockQueryable.NSubstitute;
using Moq;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.ConfirmEmail;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace UnitTests.Commands;

public sealed class ConfirmEmailCommandTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private SqliteContext _context = null!;

    public ConfirmEmailCommandTests()
    {
        _authServiceMock = new Mock<IAuthService>();
    }

    [Fact]
    public async Task CantConfirmEmailIfUserDoesNotExists()
    {
        // Arrange
        _authServiceMock
            .Setup(a => a.GetUserIdFromConfirmationToken(It.IsAny<string>()))
            .ReturnsAsync(new UserId());
        var commandHandler = new ConfirmEmailCommandHandler(_authServiceMock.Object, _context, null!);

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

        var userDbSetMock = new[] { user }.AsQueryable().BuildMockDbSet();
        var contextMock = new Mock<ISiteWatcherContext>();
        contextMock.Setup(c => c.Users).Returns(userDbSetMock);

        _authServiceMock
            .Setup(a => a.GetUserIdFromConfirmationToken(It.IsAny<string>()))
            .ReturnsAsync(user.Id);

        var sessionMock = new Mock<ISession>();
        var commandHandler = new ConfirmEmailCommandHandler(_authServiceMock.Object, contextMock.Object, sessionMock.Object);

        // Act
        var result = await commandHandler.Handle(new ConfirmEmailCommand {Token = "INVALID_TOKEN"}, CancellationToken.None) as ErrorResult;

        // Assert

        result!.Errors
            .Count().Should().Be(1);

        result.Errors.First()
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ConfirmEmailCommand.Token)));

        user.EmailConfirmed.Should().BeFalse();
        user.SecurityStamp.Should().BeNull();
    }
}