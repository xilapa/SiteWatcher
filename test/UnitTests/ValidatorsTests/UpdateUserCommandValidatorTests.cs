using FluentAssertions;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.Enums;

namespace UnitTests.ValidatorsTests;

public class UpdateUserCommandValidatorTests
{
    public static IEnumerable<object[]> UpdateUserData()
    {
        yield return new object[]
        {
            new UpdateUserCommand
            {
                Email = "",
                Language = default,
                Name = "",
                Theme = default
            },
            new []
            {
                ApplicationErrors.NAME_NOT_BE_NULL_OR_EMPTY, ApplicationErrors.EMAIL_NOT_BE_NULL_OR_EMPTY,
                ApplicationErrors.LANGUAGE_IS_INVALID, ApplicationErrors.THEME_IS_INVALID
            }
        };

        yield return new object[]
        {
            new UpdateUserCommand
            {
                Email = "invalidEmail",
                Language = ELanguage.English,
                Name = "Xi",
                Theme = ETheme.Dark
            },
            new [] {ApplicationErrors.NAME_MINIMUM_LENGTH, ApplicationErrors.EMAIL_IS_INVALID}
        };

        yield return new object[]
        {
            new UpdateUserCommand
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
    [MemberData(nameof(UpdateUserData))]
    public async Task Teste(UpdateUserCommand command, string[] messages)
    {
        // Arrange
        var validator = new UpdateUserCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.Errors.Select(e => e.ErrorMessage)
            .Should().BeEquivalentTo(messages);
    }
}