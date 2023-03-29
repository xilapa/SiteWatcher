using Domain.Authentication;
using Moq;
using SiteWatcher.Application.Authentication.Commands.Authentication;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Repositories;

namespace UnitTests.Commands;

public sealed class AuthenticationCommandTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private const string RegisterToken = nameof(RegisterToken);
    private const string GoogleId = nameof(GoogleId);
    private const string Email = nameof(Email);

    public AuthenticationCommandTests()
    {
        _authServiceMock = new Mock<IAuthService>();

        _authServiceMock
            .Setup(a => a.GenerateRegisterToken(It.IsAny<UserRegisterData>()))
            .Returns(RegisterToken);

        _authServiceMock
            .Setup(a => a.StoreAuthenticationResult(It.IsAny<AuthenticationResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthKeys(RegisterToken, RegisterToken));
    }

    [Fact]
    public async Task RegisterTokenIsGeneratedWhenUserDoesNotExists()
    {
        // Arrange
        var userDapperRepoMock = new Mock<IUserDapperRepository>();
        userDapperRepoMock
            .Setup(_ => _.GetUserByGoogleIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as UserViewModel);

        var commandHandler = new AuthenticationCommandHandler(userDapperRepoMock.Object, _authServiceMock.Object);

        var command = new AuthenticationCommand { GoogleId = GoogleId, Email = Email };

        // Act
        await commandHandler.Handle(command, default);

        // Assert
        _authServiceMock
            .Verify(a => a.GenerateRegisterToken(It.IsAny<UserRegisterData>()), Times.Once);
        _authServiceMock
            .Verify(a => a.GenerateLoginToken(It.IsAny<UserViewModel>()), Times.Never);
        _authServiceMock
            .Verify(a =>
                    a.StoreAuthenticationResult(It.IsAny<AuthenticationResult>(), It.IsAny<CancellationToken>()),
                Times.Once);
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

        var commandHandler =
            new AuthenticationCommandHandler(userDapperRepoMock.Object, _authServiceMock.Object);

        var command = new AuthenticationCommand { GoogleId = GoogleId, Email = Email };

        // Act
        await commandHandler.Handle(command, default);

        // Assert
        _authServiceMock
            .Verify(a => a.GenerateRegisterToken(It.IsAny<UserRegisterData>()), Times.Never);
        _authServiceMock
            .Verify(a => a.GenerateLoginToken(It.IsAny<UserViewModel>()), Times.Never);
        _authServiceMock
            .Verify(a =>
                    a.StoreAuthenticationResult(It.IsAny<AuthenticationResult>(), It.IsAny<CancellationToken>()),
                Times.Once);
    }
}