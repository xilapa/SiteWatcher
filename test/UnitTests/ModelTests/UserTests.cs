using FluentAssertions;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models;

namespace UnitTests.ModelTests;

public class UserTests
{
    [Theory]
    [InlineData("email", "email", true)]
    [InlineData("email", "another-email", false)]
    public void UserEmailConfirmedAfterCreation(string email, string authEmail, bool emailConfirmed)
    {
        // Arrange
        // Act
        var user = new User("googleId", "name", email, authEmail, ELanguage.English, ETheme.Dark);
        
        // Assert
        user.EmailConfirmed.Should().Be(emailConfirmed);
    }
    
    [Theory]
    [InlineData("email", "email", "email", true)] // email == authEmail
    [InlineData("email", "email", "another-email", false)] // email == authEmail
    [InlineData("email", "another-email", "email", false)] // email != authEmail
    public void UserEmailConfirmedAfterUpdate(string email, string authEmail, string newEmail, bool emailConfirmed)
    {
        // Arrange
        var user = new User("googleId", "name", email, authEmail, ELanguage.English, ETheme.Dark);
        var userUpdate = new UpdateUserInput()
        {
            Name = "name",
            Email = newEmail,
            Language = ELanguage.English,
            Theme = ETheme.Dark
        };
        
        // Act
        user.Update(userUpdate, DateTime.Now);
        
        // Assert
        user.EmailConfirmed.Should().Be(emailConfirmed);
    }
}