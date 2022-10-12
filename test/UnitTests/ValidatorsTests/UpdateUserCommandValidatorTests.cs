using FluentAssertions;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.Enums;

namespace UnitTests.ValidatorsTests;

public sealed class UpdateUserCommandValidatorTests
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
                ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateUserCommand.Name)),
                ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateUserCommand.Email)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Language)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Theme))
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
            new []
            {
                ApplicationErrors.ValueBellowMinimumLength(nameof(UpdateUserCommand.Name)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Email))
            }
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

        yield return new object[]
        {
            new UpdateUserCommand
            {
                Email = "xilapa@email.com",
                Language = (ELanguage) 989,
                Name = "xilapa",
                Theme = (ETheme) 989
            },
            new []
            {
                ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Language)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Theme))
            }
        };
    }

    [Theory]
    [MemberData(nameof(UpdateUserData))]
    public async Task Test(UpdateUserCommand command, string[] messages)
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