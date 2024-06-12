using FluentAssertions;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Alerts.Enums;

namespace UnitTests.ValidatorsTests;

public sealed class CreateAlertCommandValidatorTests
{
    public static IEnumerable<object[]> CreateAlertData()
    {
        yield return new object[]
        {
            new CreateAlertCommand
            {
                Name = "ab",
                Frequency = default,
                SiteName = "ab",
                SiteUri = "invalid-uri",
                Rule = default
            },
            new[]
            {
                ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.Name)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)),
                ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.SiteName)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.SiteUri)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Rule))
            }
        };

        yield return new object[]
        {
            new CreateAlertCommand
            {
                Name = "abcdefghijklmnopqrstuvxzwyk1234567890abcdefghijklmnopqrstuvxzwyk1",
                Frequency = (Frequencies) 989,
                SiteName = "abcdefghijklmnopqrstuvxzwyk1234567890abcdefghijklmnopqrstuvxzwyk1",
                SiteUri = "https://valid-uri.io",
                Rule = RuleType.Term,
                Term = "ab"
            },
            new[]
            {
                ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.Name)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)),
                ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.SiteName)),
                ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.Term))
            }
        };

        yield return new object[]
        {
            new CreateAlertCommand
            {
                Name = "abcd",
                Frequency = Frequencies.EightHours,
                SiteName = "abcd",
                SiteUri = "https://valid-uri.io",
                Rule = RuleType.Term,
                Term = "abcdefghijklmnopqrstuvxzwyk1234567890abcdefghijklmnopqrstuvxzwyk1"
            },
            new[]
            {
                ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.Term))
            }
        };

        yield return new object[]
        {
            new CreateAlertCommand
            {
                Name = "abcd",
                Frequency = Frequencies.EightHours,
                SiteName = "abcd",
                SiteUri = "https://valid-uri.io",
                Rule = (RuleType) 989,
                Term = "abcd"
            },
            new[]
            {
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Rule))
            }
        };
    }

    [Theory]
    [MemberData(nameof(CreateAlertData))]
    public async Task CreateAlertReturnCorrectErrors(CreateAlertCommand command, string[] errorMessages)
    {
        // Arrange
        var validator = new CreateAlertCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.Errors.Select(e => e.ErrorMessage)
            .Should().BeEquivalentTo(errorMessages,
                opt => opt.WithoutStrictOrdering());
    }
}