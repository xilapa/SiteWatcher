using Domain.DTOs.Common;
using FluentAssertions;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Enums;

namespace UnitTests.ValidatorsTests;

public class UpdateAlertCommandValidatorTests
{
    public static IEnumerable<object[]> UpdateAlertData()
    {
        yield return new object[]
        {
            new UpdateAlertCommmand(),
            new[]
            {
                ApplicationErrors.UPDATE_DATA_IS_NULL,
                ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.AlertId))
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand { AlertId = "alertId"},
            new[]
            {
                ApplicationErrors.UPDATE_DATA_IS_NULL
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = "alertId",
                Name = new UpdateInfo<string>(),
                Frequency = new UpdateInfo<EFrequency>(),
                SiteName = new UpdateInfo<string>(),
                SiteUri = new UpdateInfo<string>(),
                WatchMode = new UpdateInfo<EWatchMode>()
            },
            new[]
            {
                ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.Name)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.Frequency)),
                ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.SiteName)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.SiteUri)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.WatchMode))
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = "alertId",
                Name = new UpdateInfo<string>("ab"),
                Frequency = new UpdateInfo<EFrequency>(default),
                SiteName = new UpdateInfo<string>("ab"),
                SiteUri = new UpdateInfo<string>("ab"),
                WatchMode = new UpdateInfo<EWatchMode>(default)
            },
            new[]
            {
                ApplicationErrors.ValueBellowMinimumLength(nameof(UpdateAlertCommmand.Name)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.Frequency)),
                ApplicationErrors.ValueBellowMinimumLength(nameof(UpdateAlertCommmand.SiteName)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.SiteUri)),
                ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.WatchMode))
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = "alertId",
                Name = new UpdateInfo<string>("abcdefghijklmnopqrstuvxzwyk1234567890abcdefghijklmnopqrstuvxzwyk1"),
                Frequency = new UpdateInfo<EFrequency>(EFrequency.EightHours),
                SiteName = new UpdateInfo<string>("abcdefghijklmnopqrstuvxzwyk1234567890abcdefghijklmnopqrstuvxzwyk1"),
                SiteUri = new UpdateInfo<string>("http://site.new"),
                WatchMode = new UpdateInfo<EWatchMode>(EWatchMode.Term)
            },
            new[]
            {
                ApplicationErrors.ValueAboveMaximumLength(nameof(UpdateAlertCommmand.Name)),
                ApplicationErrors.ValueAboveMaximumLength(nameof(UpdateAlertCommmand.SiteName)),
                ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.Term))
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = "alertId",
                Name = new UpdateInfo<string>("abcde"),
                Frequency = new UpdateInfo<EFrequency>(EFrequency.EightHours),
                SiteName = new UpdateInfo<string>("abcde"),
                SiteUri = new UpdateInfo<string>("http://site.new"),
                WatchMode = new UpdateInfo<EWatchMode>(EWatchMode.Term),
                Term = new UpdateInfo<string?>()
            },
            new[]
            {
                ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.Term))
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = "alertId",
                Name = new UpdateInfo<string>("abcde"),
                Frequency = new UpdateInfo<EFrequency>(EFrequency.EightHours),
                SiteName = new UpdateInfo<string>("abcde"),
                SiteUri = new UpdateInfo<string>("http://site.new"),
                WatchMode = new UpdateInfo<EWatchMode>(EWatchMode.Term),
                Term = new UpdateInfo<string?>(null)
            },
            new[]
            {
                ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.Term))
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = "alertId",
                Name = new UpdateInfo<string>("abcde"),
                Frequency = new UpdateInfo<EFrequency>(EFrequency.EightHours),
                SiteName = new UpdateInfo<string>("abcde"),
                SiteUri = new UpdateInfo<string>("http://site.new"),
                WatchMode = new UpdateInfo<EWatchMode>(EWatchMode.Term),
                Term = new UpdateInfo<string?>("term")
            },
            Array.Empty<string>()
        };
    }

    [Theory]
    [MemberData(nameof(UpdateAlertData))]
    public async Task UpdateAlertReturnCorrectErrors(UpdateAlertCommmand command, string[] errorMessages)
    {
        // Arrange
        var validator = new UpdateAlertCommmandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.Errors.Select(e => e.ErrorMessage)
            .Should().BeEquivalentTo(errorMessages,
                opt => opt.WithoutStrictOrdering());
    }
}