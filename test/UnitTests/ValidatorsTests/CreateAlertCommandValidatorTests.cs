﻿using FluentAssertions;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Enums;

namespace UnitTests.ValidatorsTests;

public class CreateAlertCommandValidatorTests
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
                WatchMode = default
            },
            new[]
            {
                ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.Name)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)),
                ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.SiteName)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.SiteUri)),
                ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.WatchMode))
            }
        };

        yield return new object[]
        {
            new CreateAlertCommand
            {
                Name = "abcdefghijklmnopqrstuvxzwyk1234567890abcdefghijklmnopqrstuvxzwyk1",
                Frequency = EFrequency.EightHours,
                SiteName = "abcdefghijklmnopqrstuvxzwyk1234567890abcdefghijklmnopqrstuvxzwyk1",
                SiteUri = "https://valid-uri.io",
                WatchMode = EWatchMode.Term,
                Term = "ab"
            },
            new[]
            {
                ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.Name)),
                ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.SiteName)),
                ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.Term))
            }
        };

        yield return new object[]
        {
            new CreateAlertCommand
            {
                Name = "abcd",
                Frequency = EFrequency.EightHours,
                SiteName = "abcd",
                SiteUri = "https://valid-uri.io",
                WatchMode = EWatchMode.Term,
                Term = "abcdefghijklmnopqrstuvxzwyk1234567890abcdefghijklmnopqrstuvxzwyk1"
            },
            new[]
            {
                ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.Term))
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