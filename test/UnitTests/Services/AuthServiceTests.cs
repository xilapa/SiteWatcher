using System.Reflection;
using System.Runtime.CompilerServices;
using Moq;
using ReflectionMagic;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.Authorization;

namespace UnitTests.Services;

public sealed class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly int _loginTokenExpiration;
    private readonly UserId _userId;
    private const string WhitelistedTokenPayload = "WHITELISTED_TOKEN_PAYLOAD";

    public AuthServiceTests()
    {
        _authService = (RuntimeHelpers.GetUninitializedObject(typeof(AuthService)) as AuthService)!;

        _loginTokenExpiration = (int) typeof(AuthService)
            .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(f => f.Name == "LoginTokenExpiration")
            .GetValue(_authService)!;

        _userId = new UserId(new Guid("00000000-0000-0000-0000-000000002222"));
        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(s => s.UserId).Returns(_userId);
        sessionMock.Setup(s => s.AuthTokenPayload).Returns(WhitelistedTokenPayload);

        _authService.AsDynamic()._session = sessionMock.Object;
    }

    [Fact]
    public async Task TokenIsRemovedFromWhitelistWhenLogoutFromAllDevices()
    {
        // Arrange
        var cacheMock = new Mock<ICache>();
        var key = CacheKeys.InvalidUser(_userId);
        cacheMock.Setup(c => c.GetAsync<List<string>>(key))
            .ReturnsAsync(new List<string> {WhitelistedTokenPayload});

        _authService.AsDynamic()._cache = cacheMock.Object;

        // Act
        await _authService.InvalidateCurrenUser();

        // Assert
        cacheMock.Verify(c => c.GetAsync<List<string>>(key), Times.Once);

        cacheMock.Verify(c =>
            c.SaveAsync(key,
                It.Is<List<string>>(l => l.Count == 0),
                TimeSpan.FromSeconds(_loginTokenExpiration)));
    }

    [Fact]
    public async Task TokenWhitelistIsCreatedEmpty()
    {
        // Arrange
        var cacheMock = new Mock<ICache>();
        var key = CacheKeys.InvalidUser(_userId);
        cacheMock.Setup(c => c.GetAsync<List<string>>(key))
            .Returns(Task.FromResult<List<string>>(null!)!);

        _authService.AsDynamic()._cache = cacheMock.Object;

        // Act
        await _authService.InvalidateCurrenUser();

        // Assert
        cacheMock.Verify(c => c.GetAsync<List<string>>(key), Times.Once);

        cacheMock.Verify(c =>
            c.SaveAsync(key,
                It.Is<List<string>>(l => l.Count == 0),
                TimeSpan.FromSeconds(_loginTokenExpiration)));
    }
}