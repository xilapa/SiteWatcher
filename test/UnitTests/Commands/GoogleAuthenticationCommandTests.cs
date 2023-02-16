using System.Security.Claims;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Authentication.Common;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Repositories;

namespace UnitTests.Commands;

public sealed class GoogleAuthenticationCommandTests
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
            .Setup(_ => _.GetUserByGoogleIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as UserViewModel);

        var commandHandler = new GoogleAuthenticationCommandHandler(_googleAuthService, userDapperRepoMock.Object, _authService);

        var command = new GoogleAuthenticationCommand();

        // Act
        var result = await commandHandler.Handle(command, default) as ValueResult<AuthenticationResult>;

        // Assert
        result!.Value.Task.Should().Be(AuthTask.Register);
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
            .Setup(_ => _.GetUserByGoogleIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userVm);

        var commandHandler = new GoogleAuthenticationCommandHandler(_googleAuthService, userDapperRepoMock.Object, _authService);

        var command = new GoogleAuthenticationCommand();

        // Act
        var result = await commandHandler.Handle(command, default) as ValueResult<AuthenticationResult>;

        // Assert
        result!.Value.Task.Should().Be(AuthTask.Activate);
        result.Value.Token.Should().Be(userVm.Id.ToString());
    }
}