using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;

namespace UnitTests.ValidatorsTests;

public sealed class GoogleAuthenticationCommandValidatorTests
{
    private readonly ILogger<GoogleAuthenticationCommandValidator> _logger;
    private readonly  IGoogleSettings _googleSettings;
    private readonly Mock<ICache> _cacheMock;
    private readonly byte[] _validState = {1, 2, 3, 4, 5};
    private const string Scopes = "SCOPEA SCOPEB SCOPEC";

    public GoogleAuthenticationCommandValidatorTests()
    {
        _logger = new Mock<ILogger<GoogleAuthenticationCommandValidator>>().Object;
        var googleSettingsMock = new Mock<IGoogleSettings>();
        googleSettingsMock
            .Setup(_ => _.Scopes)
            .Returns(Scopes);
        googleSettingsMock
            .Setup(_ => _.StateValue)
            .Returns(_validState);
        _googleSettings = googleSettingsMock.Object;
        _cacheMock= new Mock<ICache>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("SCOPEA")]
    [InlineData("SCOPEA SCOPEB")]
    public async Task MissingScopeReturnValidationError(string scope)
    {
        // Arrange
        _cacheMock
            .Setup(_ => _.GetAndRemoveBytesAsync(It.IsAny<string>()))
            .ReturnsAsync(_validState);

        var command = new GoogleAuthenticationCommand {Scope = scope, Code = "code", State = "state"};
        var validator = new GoogleAuthenticationCommandValidator(_googleSettings, _cacheMock.Object, _logger);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be(ApplicationErrors.GOOGLE_AUTH_ERROR);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new byte[] {1, 2, 3})]
    public async Task InvalidStateReturnValidationError(byte[] state)
    {
        // Arrange
        _cacheMock
            .Setup(_ => _.GetAndRemoveBytesAsync(It.IsAny<string>()))
            .ReturnsAsync(state);

        var command = new GoogleAuthenticationCommand {State = "STATE", Scope = Scopes, Code = "code"};
        var validator = new GoogleAuthenticationCommandValidator(_googleSettings, _cacheMock.Object, _logger);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be(ApplicationErrors.GOOGLE_AUTH_ERROR);
    }
}