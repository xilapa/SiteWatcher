using System.Data;
using Domain.Authentication;
using FluentAssertions;
using Moq;
using SiteWatcher.Application.Authentication.Commands.Authentication;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Infra.DapperRepositories;

namespace UnitTests.Commands;

public sealed class AuthenticationCommandTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly IQueries _queries;
    private const string RegisterToken = nameof(RegisterToken);
    private const string GoogleId = nameof(GoogleId);
    private const string Email = nameof(Email);
    private const string CodeChallenge = nameof(CodeChallenge);

    public AuthenticationCommandTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _queries = new PostgresQueries();

        _authServiceMock
            .Setup(a => a.GenerateRegisterToken(It.IsAny<UserRegisterData>()))
            .Returns(RegisterToken);

        _authServiceMock
            .Setup(a => a.StoreAuthenticationResult(It.IsAny<AuthenticationResult>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthCodeResult(RegisterToken, RegisterToken));
    }

    [Fact]
    public async Task RegisterTokenIsGeneratedWhenUserDoesNotExists()
    {
        // Arrange
        var contextMock = new Mock<IDapperContext>();

        var commandHandler = new AuthenticationCommandHandler(contextMock.Object, _queries, _authServiceMock.Object);

        var command = new AuthenticationCommand { GoogleId = GoogleId, Email = Email, CodeChallenge = CodeChallenge };

        // Act
        await commandHandler.Handle(command, default);

        // Assert
        _authServiceMock
            .Verify(a => a.GenerateRegisterToken(It.IsAny<UserRegisterData>()), Times.Once);
        _authServiceMock
            .Verify(a => a.GenerateLoginToken(It.IsAny<UserViewModel>()), Times.Never);
        _authServiceMock
            .Verify(a =>
                    a.StoreAuthenticationResult(It.IsAny<AuthenticationResult>(), It.IsAny<string>(),It.IsAny<CancellationToken>()),
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
        var userDapperRepoMock = new Mock<IDapperContext>();
        userDapperRepoMock
            .Setup(_ => _.UsingConnectionAsync(It.IsAny<Func<IDbConnection,Task<UserViewModel?>>>()))
            .ReturnsAsync(userVm);

        var commandHandler =
            new AuthenticationCommandHandler(userDapperRepoMock.Object, _queries, _authServiceMock.Object);

        var command = new AuthenticationCommand { GoogleId = GoogleId, Email = Email, CodeChallenge = CodeChallenge};

        // Act
        await commandHandler.Handle(command, default);

        // Assert
        _authServiceMock
            .Verify(a => a.GenerateRegisterToken(It.IsAny<UserRegisterData>()), Times.Never);
        _authServiceMock
            .Verify(a => a.GenerateLoginToken(It.IsAny<UserViewModel>()), Times.Never);
        _authServiceMock
            .Verify(a =>
                    a.StoreAuthenticationResult(It.IsAny<AuthenticationResult>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task InvalidAuthCommandReturnError()
    {
        // Arrange
        var commandHandler = new AuthenticationCommandHandler(null!, null!, _authServiceMock.Object);

        var command = new AuthenticationCommand();

        // Act
        var res = await commandHandler.Handle(command, default);

        // Assert
        res.Code.Should().BeNull();
        res.Success().Should().BeFalse();
        res.ErrorMessage.Should().Be(ApplicationErrors.GOOGLE_AUTH_ERROR);
    }
}