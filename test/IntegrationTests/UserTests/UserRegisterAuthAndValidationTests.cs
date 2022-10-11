using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using MediatR;
using Moq;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Domain.Enums;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.UserTests;

public sealed class UserRegisterAuthAndValidationTestsBase : BaseTestFixture
{
    public readonly Registered RegisteredUserResult = new("REGISTER_TOKEN", true);

    public override Action<CustomWebApplicationOptions> Options => opts =>
    {
        var registerUserHandlerMock = new Mock<IRequestHandler<RegisterUserCommand, RegisterUserResult>>();

        registerUserHandlerMock.Setup(h =>
                h.Handle(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RegisteredUserResult);

        opts.ReplaceService(typeof(IRequestHandler<RegisterUserCommand, RegisterUserResult>), registerUserHandlerMock.Object);
    };
}

public sealed class UserRegisterAuthAndValidationTests : BaseTest, IClassFixture<UserRegisterAuthAndValidationTestsBase>
{
    private readonly UserRegisterAuthAndValidationTestsBase _fixture;
    private readonly RegisterUserCommand _registerUserCommand;

    public UserRegisterAuthAndValidationTests(UserRegisterAuthAndValidationTestsBase fixture) : base(fixture)
    {
        _fixture = fixture;
        _registerUserCommand  = new RegisterUserCommand
        {
            Email = "email@email.com",
            Language = ELanguage.English,
            Name = "Xilapilson",
            Theme = ETheme.Dark
        };
    }

    [Fact]
    public async Task InvalidRegisterTokenReturnsUnauthorized()
    {
        // Arrange
        RemoveLoginToken();

        // Act
        var result = await PostAsync("user/register", _registerUserCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);

        result.HttpMessageContent
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task ValidRegisterTokenWithValidDataReturnsCreated()
    {
        // Arrange
        SetRegisterToken(Users.Xilapa);

        // Act
        var result = await PostAsync("user/register", _registerUserCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        result.GetTyped<Registered>()
            .Should()
            .BeEquivalentTo(_fixture.RegisteredUserResult);
    }

    public static IEnumerable<object[]> InvalidRegisterData()
    {
        yield return new object[]
        {
            new RegisterUserCommand
            {
                Email = "",
                Language = default,
                Name = "",
                Theme = default
            },
            new []
            {
                ApplicationErrors.ValueIsNullOrEmpty(nameof(RegisterUserCommand.Name)),
                ApplicationErrors.ValueIsNullOrEmpty(nameof(RegisterUserCommand.Email)),
                ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Language)),
                ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Theme))
            }
        };

        yield return new object[]
        {
            new RegisterUserCommand
            {
                Email = "invalidEmail",
                Language = ELanguage.English,
                Name = "Xi",
                Theme = ETheme.Dark
            },
            new []
            {
                ApplicationErrors.ValueBellowMinimumLength(nameof(RegisterUserCommand.Name)),
                ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Email))
            }
        };

        yield return new object[]
        {
            new RegisterUserCommand
            {
                Email = "xilapa@email.com",
                Language = ELanguage.English,
                Name = "Xil4pa4",
                Theme = ETheme.Dark
            },
            new [] {ApplicationErrors.NAME_MUST_HAVE_ONLY_LETTERS}
        };
    }

    [Theory]
    [MemberData(nameof(InvalidRegisterData))]
    public async Task ValidRegisterTokenWithInvalidDataReturnsBadRequest(RegisterUserCommand command, string[] messages)
    {
        // Arrange
        SetRegisterToken(Users.Xilapa);

        // Act
        var result = await PostAsync("user/register", command);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        result.GetTyped<string[]>()
            .Should().BeEquivalentTo(messages, opt => opt.WithoutStrictOrdering());
    }
}