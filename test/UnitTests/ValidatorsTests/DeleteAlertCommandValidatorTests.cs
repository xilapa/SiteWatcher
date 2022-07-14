using FluentAssertions;
using SiteWatcher.Application.Alerts.Commands.DeleteAlert;
using SiteWatcher.Application.Common.Constants;

namespace UnitTests.ValidatorsTests;

public class DeleteAlertCommandValidatorTests
{
    [Fact]
    public async Task CannotPassEmptyIdToDelete()
    {
        // Arrange
        var command = new DeleteAlertCommand {AlertId = string.Empty};
        var validator = new DeleteAlertCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors[0].ErrorMessage.Should()
            .Be(ApplicationErrors.ValueIsNullOrEmpty(nameof(DeleteAlertCommand.AlertId)));
    }
}