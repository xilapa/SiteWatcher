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
        var registerInput = new RegisterUserInput("name", email, Language.BrazilianPortuguese, Theme.Light,
            "googleId", authEmail);
        var (user, @event) = User.Create(registerInput, DateTime.Now);

        // Assert
        user.EmailConfirmed.Should().Be(emailConfirmed);

        // Check email confirmation message
        if (!emailConfirmed)
            @event.Should().NotBeNull();
        else
            @event.Should().BeNull();
    }

    [Theory]
    [InlineData("email", "email", "email", true)] // email == authEmail
    [InlineData("email", "email", "another-email", false)] // email == authEmail
    [InlineData("email", "another-email", "email", false)] // email != authEmail
    public void UserEmailConfirmedAfterUpdate(string email, string authEmail, string newEmail, bool emailConfirmed)
    {
        // Arrange
        var registerInput = new RegisterUserInput("name", email, Language.BrazilianPortuguese, Theme.Light,
            "googleId", authEmail);
        var (user, _) = User.Create(registerInput, DateTime.Now);
        var userUpdate = new UpdateUserInput
        {
            Name = "name",
            Email = newEmail,
            Language = Language.English,
            Theme = Theme.Dark
        };

        // Act
        var (_, @event) = user.Update(userUpdate, DateTime.Now);

        // Assert
        user.EmailConfirmed.Should().Be(emailConfirmed);

        // Check email confirmation message
        if (!emailConfirmed)
            @event.Should().NotBeNull();
        else
            @event.Should().BeNull();
    }

    [Fact]
    public void UserShouldNotHaveEmptyGuidWhenCreated()
    {
        // Arrange

        // Act
        var registerInput = new RegisterUserInput("name", "email", Language.BrazilianPortuguese, Theme.Light,
            "googleId", "authEmail");
        var (user, _) = User.Create(registerInput, DateTime.Now);

        // Assert
        Guid.Empty
            .Equals(user.Id.Value)
            .Should().BeFalse();
    }
}