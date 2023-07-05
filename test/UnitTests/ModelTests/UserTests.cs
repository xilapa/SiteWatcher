using FluentAssertions;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Users.Messages;

namespace UnitTests.ModelTests;

public sealed class UserTests
{
    [Theory]
    [InlineData("email", "email", true)]
    [InlineData("email", "another-email", false)]
    public void UserEmailConfirmedAfterCreation(string email, string authEmail, bool emailConfirmed)
    {
        // Arrange
        // Act
        var user = new User("googleId", "name", email, authEmail, Language.English, Theme.Dark, DateTime.Now);

        // Assert
        user.EmailConfirmed.Should().Be(emailConfirmed);

        // Check email confirmation message
        if (!emailConfirmed)
            user.DomainEvents.Should().ContainSingle(e => e is EmailConfirmationTokenGeneratedMessage);
        else
            user.DomainEvents.Should().BeEmpty();
    }

    [Theory]
    [InlineData("email", "email", "email", true)] // email == authEmail
    [InlineData("email", "email", "another-email", false)] // email == authEmail
    [InlineData("email", "another-email", "email", false)] // email != authEmail
    public void UserEmailConfirmedAfterUpdate(string email, string authEmail, string newEmail, bool emailConfirmed)
    {
        // Arrange
        var user = new User("googleId", "name", email, authEmail, Language.English, Theme.Dark, DateTime.Now);
        user.ClearDomainEvents();
        var userUpdate = new UpdateUserInput
        {
            Name = "name",
            Email = newEmail,
            Language = Language.English,
            Theme = Theme.Dark
        };

        // Act
        user.Update(userUpdate, DateTime.Now);

        // Assert
        user.EmailConfirmed.Should().Be(emailConfirmed);

        // Check email confirmation message
        if (!emailConfirmed)
            user.DomainEvents.Should().ContainSingle(e => e is EmailConfirmationTokenGeneratedMessage);
        else
            user.DomainEvents.Should().NotContain(e => e is EmailConfirmationTokenGeneratedMessage);
    }

    [Fact]
    public void UserShouldNotHaveEmptyGuidWhenCreated()
    {
        // Arrange

        // Act
        var user = new User("googleId", "name", "email", "email",
            Language.English, Theme.Dark, DateTime.UtcNow);

        // Assert
        Guid.Empty
            .Equals(user.Id.Value)
            .Should().BeFalse();
    }
}