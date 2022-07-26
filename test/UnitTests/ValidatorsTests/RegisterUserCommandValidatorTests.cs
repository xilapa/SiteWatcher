using FluentAssertions;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Domain.Enums;

namespace UnitTests.ValidatorsTests;

public class RegisterUserCommandValidatorTests
{
        public static IEnumerable<object[]> RegisterUserData()
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

        yield return new object[]
        {
            new RegisterUserCommand
            {
                Email = "xilapa@email.com",
                Language = (ELanguage) 989,
                Name = "xilapa",
                Theme = (ETheme) 989
            },
            new []
            {
                ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Language)),
                ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Theme))
            }
        };
    }

    [Theory]
    [MemberData(nameof(RegisterUserData))]
    public async Task Test(RegisterUserCommand command, string[] messages)
    {
        // Arrange
        var validator = new RegisterUserCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.Errors.Select(e => e.ErrorMessage)
            .Should().BeEquivalentTo(messages);
    }
}