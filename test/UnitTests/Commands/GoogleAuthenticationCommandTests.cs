﻿using System.Security.Claims;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Authentication.Common;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Models.Common;

namespace UnitTests.Commands;

public class GoogleAuthenticationCommandTests
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IAuthService _authService;
    private const string RegisterToken = "REGISTER_TOKEN";

    public GoogleAuthenticationCommandTests()
    {
        var googleAuthServiceMock = new Mock<IGoogleAuthService>();
        googleAuthServiceMock
            .Setup(_ => _.ExchangeCode(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleTokenResult("id", null, new List<Claim>()));
        _googleAuthService = googleAuthServiceMock.Object;

        var authServiceMock = new Mock<IAuthService>();
        authServiceMock
            .Setup(_ => _.GenerateRegisterToken(It.IsAny<IEnumerable<Claim>>(), It.IsAny<string>()))
            .Returns(RegisterToken);

        _authService = authServiceMock.Object;
    }

    [Fact]
    public async Task RegisterTokenIsGeneratedWhenUserDoesNotExists()
    {
        // Arrange
        var userDapperRepoMock = new Mock<IUserDapperRepository>();
        userDapperRepoMock
            .Setup(_ => _.GetUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserViewModel());

        var commandHandler = new GoogleAuthenticationCommandHandler(_googleAuthService, userDapperRepoMock.Object, _authService);

        var command = new GoogleAuthenticationCommand();

        // Act
        var result = await commandHandler.Handle(command, default);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Task.Should().Be(EAuthTask.Register);
        result.Value.Token.Should().Be(RegisterToken);
    }

    [Fact]
    public async Task AcitvationTaskIsReturnedWhenUserIsDeactivated()
    {
        // Arrange
        var userVm = new UserViewModel
        {
            Id = UserId.New(),
            Active = false
        };
        var userDapperRepoMock = new Mock<IUserDapperRepository>();
        userDapperRepoMock
            .Setup(_ => _.GetUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userVm);

        var commandHandler = new GoogleAuthenticationCommandHandler(_googleAuthService, userDapperRepoMock.Object, _authService);

        var command = new GoogleAuthenticationCommand();

        // Act
        var result = await commandHandler.Handle(command, default);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Task.Should().Be(EAuthTask.Activate);
        result.Value.Token.Should().Be(userVm.Id.ToString());
    }
}