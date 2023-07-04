using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.UserTests;

public sealed class UserRegisterAuthAndValidationTestsBase : BaseTestFixture
{ }

public sealed class UserRegisterAuthAndValidationTests : BaseTest, IClassFixture<UserRegisterAuthAndValidationTestsBase>
{
    private readonly RegisterUserCommand _registerUserCommand;

    public UserRegisterAuthAndValidationTests(UserRegisterAuthAndValidationTestsBase fixture) : base(fixture)
    {
        _registerUserCommand  = new RegisterUserCommand
        {
            Email = "email@email.com",
            Language = Language.English,
            Name = "Xilapilson",
            Theme = Theme.Dark,
            GoogleId = "googleId"
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
        SetRegisterToken(new UserViewModel
        {
            Email = _registerUserCommand.Email!,
            Name = _registerUserCommand.Name!
        });

        // Act
        var result = await PostAsync("user/register", _registerUserCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        result.GetTyped<Registered>()
            .Should()
            .Match<Registered>(r => !r.ConfirmationEmailSend)
            .And
            .Match<Registered>(r => !string.IsNullOrEmpty(r.Token));
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
                Language = Language.English,
                Name = "Xi",
                Theme = Theme.Dark
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
                Language = Language.English,
                Name = "Xil4pa4",
                Theme = Theme.Dark
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