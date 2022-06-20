using System.Net;
using System.Reflection;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Moq;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Email;
using SiteWatcher.Domain.Utils;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using System.Runtime.CompilerServices;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Users.Commands.ConfirmEmail;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.IntegrationTests.Setup.TestServices;

namespace IntegrationTests.UserTests;

public class UserTests : BaseTest, IClassFixture<BaseTestFixture>, IAsyncLifetime
{
    private User _userXilapaWithoutChanges = null!;
    private int _emailConfirmationTokenExpiration;

    public UserTests(BaseTestFixture fixture) : base(fixture)
    { }

    public async Task InitializeAsync()
    {
        _userXilapaWithoutChanges = await WithDbContext(ctx
            => ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        var authServiceInstance = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));

        _emailConfirmationTokenExpiration = (int) typeof(AuthService)
            .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(f => f.Name == "EmailConfirmationTokenExpiration")
            .GetValue(authServiceInstance)!;
    }

    public Task DisposeAsync() =>
        Task.CompletedTask;

    [Fact]
    public async Task UserDataIsUpdated()
    {
        // Arrange
        LoginAs(Users.Xilapa);

        var updateUserCommand = new UpdateUserCommand
        {
            Email = "newemail@email.com",
            Language = ELanguage.Spanish,
            Name = "XilapaNewName",
            Theme = ETheme.Light
        };

        // Act
        var result = await PutAsync("user", updateUserCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var typedResult = result.GetTyped<WebApiResponse<UpdateUserResult>>();
        typedResult!
            .Messages.Count
            .Should().Be(0);
        typedResult
            .Result!.Token
            .Should().NotBeNullOrEmpty();

        var userFromDb = await WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.Email.Should().Be(updateUserCommand.Email);
        userFromDb.Language.Should().Be(updateUserCommand.Language);
        userFromDb.Name.Should().Be(updateUserCommand.Name);
        userFromDb.Theme.Should().Be(updateUserCommand.Theme);

        // Finalize
        await ResetTestData();
    }

    [Fact]
    public async Task UserDataIsNotUpdatedWithBadData()
    {
        // Arrange
        LoginAs(Users.Xilapa);

        var updateUserCommand = new UpdateUserCommand
        {
            Email = "newemail@email.com",
            Language = ELanguage.Spanish,
            Name = "Xilapa4NewName",
            Theme = ETheme.Light
        };

        // Act
        var result = await PutAsync("user", updateUserCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        var typedResult = result.GetTyped<WebApiResponse<UpdateUserResult>>();
        typedResult!
            .Messages
            .Should().NotBeEmpty();
        typedResult
            .Result
            .Should().BeNull();

        var userFromDb = await WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.Should().BeEquivalentTo(_userXilapaWithoutChanges);
    }

    [Fact]
    public async Task EmailConfirmationIsSentWhenUserChangesEmail()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        EmailServiceMock.Invocations.Clear();

        var updateUserCommand = new UpdateUserCommand
        {
            Email = "newemail@email.com",
            Language = ELanguage.Spanish,
            Name = "XilapaNewName",
            Theme = ETheme.Light
        };

        // Act
        var result = await PutAsync("user", updateUserCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        result.HttpResponse!.Content
            .Should()
            .NotBeNull();

        var userFromDb = await WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.EmailConfirmed.Should().BeFalse();

        // Verifying on cache
        FakeCache.Cache[userFromDb.SecurityStamp!]
            .Value
            .Should().Be(userFromDb.Id.ToString());

        FakeCache.Cache[userFromDb.SecurityStamp!]
            .Expiration.TotalSeconds
            .Should().Be(_emailConfirmationTokenExpiration);

        // Verifying that email was sent only once
        EmailServiceMock.Verify(e =>
            e.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verifying that the correct message was sent
        var link = $"{TestSettings.FrontEndUrl}/#/security/confirm-email?t={userFromDb.SecurityStamp}";
        var message =
            MailMessageGenerator.EmailConfirmation(updateUserCommand.Name, updateUserCommand.Email, link,
                updateUserCommand.Language);

        (EmailServiceMock.Invocations[0].Arguments[0] as MailMessage)
            .Should().BeEquivalentTo(message);

        // Finalize
        await ResetTestData();
    }

    [Fact]
    public async Task EmailConfirmationIsSentAfterUserCreation()
    {
        // Arrange
        EmailServiceMock.Invocations.Clear();
        var userViewModel = new UserViewModel
        {
            Name = "TestUser",
            Email = "email-test-user@newemail.com"
        };
        SetRegisterToken(userViewModel);

        var registerUserCommand = new RegisterUserCommand
        {
            Email = $"new-{userViewModel.Email}",
            Language = ELanguage.BrazilianPortuguese,
            Name = userViewModel.Name,
            Theme = ETheme.Light
        };

        // Act
        var result = await PostAsync("user/register", registerUserCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        result.HttpResponse!.Content
            .Should()
            .NotBeNull();

        var userFromDb = await WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.GoogleId == userViewModel.GetGoogleId()));

        userFromDb.EmailConfirmed.Should().BeFalse();

        // Verifying on cache
        FakeCache.Cache[userFromDb.SecurityStamp!]
            .Value
            .Should().Be(userFromDb.Id.ToString());

        FakeCache.Cache[userFromDb.SecurityStamp!]
            .Expiration.TotalSeconds
            .Should().Be(_emailConfirmationTokenExpiration);

        // Verifying that email was sent only once
        EmailServiceMock.Verify(e =>
                e.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verifying that the correct message was sent
        var link = $"{TestSettings.FrontEndUrl}/#/security/confirm-email?t={userFromDb.SecurityStamp}";
        var message =
            MailMessageGenerator.EmailConfirmation(registerUserCommand.Name, registerUserCommand.Email, link,
                registerUserCommand.Language);

        (EmailServiceMock.Invocations[0].Arguments[0] as MailMessage)
            .Should().BeEquivalentTo(message);
    }

    [Fact]
    public async Task EmailIsConfirmedWithValidToken()
    {
        // Arrange
        FakeCache.Cache.Clear();
        LoginAs(Users.Xulipa);
        const string fakeToken = "TEST_TOKEN_EMAIL_CONFIRMATION";

        // Ensuring that the user email is not confirmed
        await WithDbContext(ctx =>
                ctx.Database.ExecuteSqlRawAsync(@$"UPDATE Users 
                                                SET EmailConfirmed = 0,
                                                SecurityStamp = '{fakeToken}'
                                                WHERE Id = '{Users.Xulipa.Id}'"));

        // Setting a fake token on cache
        FakeCache.Cache[fakeToken] = new FakeCacheEntry(Users.Xulipa.Id, TimeSpan.Zero);
        var confirmEmailCommand = new ConfirmEmailCommand {Token = fakeToken};

        // Act
        var result = await PutAsync("user/confirm-email", confirmEmailCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Checking that the user email is confirmed
        var updatedUser = await WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xulipa.Id));

        updatedUser.EmailConfirmed.Should().BeTrue();

        // Verifying on cache that the token was removed
        FakeCache.Cache.Should().BeEmpty();
    }

    [Fact]
    public async Task EmailIsNotConfirmedWithInvalidToken()
    {
        // Arrange
        FakeCache.Cache.Clear();
        LoginAs(Users.Xulipa);
        const string fakeToken = "TEST_TOKEN_EMAIL_CONFIRMATION";

        // Ensuring that the user email is not confirmed
        await WithDbContext(ctx =>
            ctx.Database.ExecuteSqlRawAsync(@$"UPDATE Users 
                                                SET EmailConfirmed = 0,
                                                SecurityStamp = '{fakeToken}'
                                                WHERE Id = '{Users.Xulipa.Id}'"));

        // Setting a fake token on cache
        var fakeTokenEntry = new FakeCacheEntry(Users.Xulipa.Id, TimeSpan.Zero);
        FakeCache.Cache[fakeToken] = fakeTokenEntry;
        var confirmEmailCommand = new ConfirmEmailCommand {Token = "INVALID_TOKEN"};

        // Act
        var result = await PutAsync("user/confirm-email", confirmEmailCommand);

        result.GetTyped<WebApiResponse<object>>()!
            .Messages[0]
            .Should().Be(ApplicationErrors.INVALID_TOKEN);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        // Checking that the user email is confirmed
        var updatedUser = await WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xulipa.Id));

        updatedUser.EmailConfirmed
            .Should().BeFalse();

        // Verifying on cache that the token was removed
        FakeCache.Cache[fakeToken]
            .Should().Be(fakeTokenEntry);
    }

    private async Task ResetTestData()
    {
        await WithDbContext(async ctx =>
        {
            var user = await ctx.Users.SingleOrDefaultAsync(u => u.Id == Users.Xilapa.Id);
            if (user is not null)
                ctx.Users.Remove(user);
            ctx.Users.Add(_userXilapaWithoutChanges);
            await ctx.SaveChangesAsync();
        });

        var userReseted = await WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userReseted.Should().BeEquivalentTo(_userXilapaWithoutChanges);
    }
}