using FluentAssertions;
using SiteWatcher.Application.Alerts.Commands.SearchAlerts;
using SiteWatcher.Application.Common.Constants;

namespace UnitTests.ValidatorsTests;

public sealed class SearchAlertCommandValidatorTests
{
    public static IEnumerable<object[]> InvalidSearchTerms()
    {
        yield return new object[] {""};
        yield return new object[] {string.Empty};
        yield return new object[] {" "};
        yield return new object[] {null!};
        yield return new object[] {default!};
    }

    [Theory]
    [MemberData(nameof(InvalidSearchTerms))]
    public async Task CantSendEmptySearchTerm(string? searchTerm)
    {
        // Arrange
        var validator = new SearchAlertCommandValidator();
        var command = new SearchAlertCommand {Term = searchTerm!};

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.ErrorMessage).Should()
            .BeEquivalentTo(ApplicationErrors.ValueIsNullOrEmpty(nameof(SearchAlertCommand.Term)));
    }
}