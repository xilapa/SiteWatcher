using FluentAssertions;
using SiteWatcher.Domain.Common.Extensions;

namespace UnitTests.ExtensionsTests;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("Macarrão", "macarrao")]
    [InlineData("Wikipédia", "wikipedia")]
    public void StringIsLowerCaseAndDiacriticsAreRemoved(string input, string expected)
    {
        // Act
        var result = input.ToLowerCaseWithoutDiacritics();

        // Assert
        result.Should().Be(expected);
    }
}