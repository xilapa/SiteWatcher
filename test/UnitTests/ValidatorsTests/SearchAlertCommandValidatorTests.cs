using FluentAssertions;
using FluentValidation;
using SiteWatcher.Application.Alerts.Commands.SearchAlerts;
using SiteWatcher.Application.Common.Constants;

namespace UnitTests.ValidatorsTests;

public class SearchAlertCommandValidatorTests
{
    public static IEnumerable<object[]> InvalidSearchTerms()
    {
        yield return new string?[] {""};
        yield return new string?[] {string.Empty};
        yield return new string?[] {" "};
        yield return new string?[] {null};
        yield return new string?[] {default};
    }

    [Theory]
    [MemberData(nameof(InvalidSearchTerms))]
    public async Task CantSendEmptySearchTerm(string? searchTerm)
    {
        // Arrange
        var validator = new SearchAlertCommandValidator();
        var command = new SearchAlertCommand {Term = searchTerm};

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.ErrorMessage).Should()
            .BeEquivalentTo(ApplicationErrors.ValueIsNullOrEmpty(nameof(SearchAlertCommand.Term)));
    }
}