using System.Net;
using System.Reflection;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Moq;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Emails;
using SiteWatcher.Domain.Utils;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;
using System.Runtime.CompilerServices;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Application.Users.Commands.ConfirmEmail;
using SiteWatcher.Application.Users.Commands.ReactivateAccount;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

namespace IntegrationTests.UserTests;

public class UserTests : BaseTest, IClassFixture<BaseTestFixture>, IAsyncLifetime
{
    private User _userXilapaWithoutChanges = null!;
    private int _emailConfirmationTokenExpiration;
    private int _loginTokenExpiration;
    private int _accountReactivationTokenExpiration;

    public UserTests(BaseTestFixture fixture) : base(fixture)
    { }

    public async Task InitializeAsync()
    {
        _userXilapaWithoutChanges = await AppFactory.WithDbContext(ctx
            => ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        var authServiceInstance = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));

        _emailConfirmationTokenExpiration = (int) typeof(AuthService)
            .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(f => f.Name == "EmailConfirmationTokenExpiration")
            .GetValue(authServiceInstance)!;

        _loginTokenExpiration = (int) typeof(AuthService)
            .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(f => f.Name == "LoginTokenExpiration")
            .GetValue(authServiceInstance)!;

        _accountReactivationTokenExpiration = (int) typeof(AuthService)
            .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(f => f.Name == "AccountReactivationTokenExpiration")
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

        var typedResult = result.GetTyped<UpdateUserResult>();
        typedResult!
            .Token
            .Should().NotBeNullOrEmpty();

        var userFromDb = await AppFactory.WithDbContext(ctx =>
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

        var typedResult = result.GetTyped<string[]>();
        typedResult!
            .Should().NotBeEmpty();

        var userFromDb = await AppFactory.WithDbContext(ctx =>
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

        // Await fire and forget to execute
        await Task.Delay(300);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        result.HttpResponse!.Content
            .Should()
            .NotBeNull();

        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.EmailConfirmed.Should().BeFalse();
        userFromDb.LastUpdatedAt.Should().Be(CurrentTime);

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
        FakeCache.Cache.Clear();
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

        // Await fire and forget to execute
        await Task.Delay(300);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        result.HttpResponse!.Content
            .Should()
            .NotBeNull();

        var userFromDb = await AppFactory.WithDbContext(ctx =>
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
        await AppFactory.WithDbContext(ctx =>
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
            .Be(HttpStatusCode.NoContent);

        // Checking that the user email is confirmed
        var updatedUser = await AppFactory.WithDbContext(ctx =>
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
        await AppFactory.WithDbContext(ctx =>
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

        result.GetTyped<string[]>()![0]
            .Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ConfirmEmailCommand.Token)));

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        // Checking that the user email is not confirmed
        var updatedUser = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xulipa.Id));

        updatedUser.EmailConfirmed
            .Should().BeFalse();

        // Verifying on cache that the token was not removed
        FakeCache.Cache[fakeToken]
            .Should().Be(fakeTokenEntry);
    }

    [Fact]
    public async Task ManualEmailConfirmationIsSent()
    {
        // Arrange
        EmailServiceMock.Invocations.Clear();
        FakeCache.Cache.Clear();
        LoginAs(Users.Xulipa);
        // Ensuring that the user email is not confirmed
        await AppFactory.WithDbContext(ctx =>
            ctx.Database.ExecuteSqlRawAsync(@$"UPDATE Users 
                                                SET EmailConfirmed = 0
                                                WHERE Id = '{Users.Xulipa.Id}'"));

        // Act
        var result = await PutAsync("user/resend-confirmation-email");

        // Await fire and forget to execute
        await Task.Delay(300);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Verifying that the user was updated on database
        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xulipa.Id));

        userFromDb.SecurityStamp.Should().NotBeNull();
        userFromDb.LastUpdatedAt.Should().Be(CurrentTime);

        // Verifying that the cache entry was created
        FakeCache.Cache
            .Values.Count.Should().Be(1);

        FakeCache.Cache[userFromDb.SecurityStamp!]
            .Value
            .Should().Be(userFromDb.Id.ToString());

        // Verifying that email was sent only once
        EmailServiceMock.Verify(e =>
                e.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verifying that the correct message was sent
        var link = $"{TestSettings.FrontEndUrl}/#/security/confirm-email?t={userFromDb.SecurityStamp}";
        var message =
            MailMessageGenerator.EmailConfirmation(Users.Xulipa.Name, Users.Xulipa.Email, link,
                Users.Xulipa.Language);

        (EmailServiceMock.Invocations[0].Arguments[0] as MailMessage)
            .Should().BeEquivalentTo(message);
    }

    [Fact]
    public async Task UserNeedsToLoginAgainAfterLogoutFromAllDevices()
    {
        // Arrange
        FakeCache.Cache.Clear();
        LoginAs(Users.Xilapa);
        var cachekey = CacheKeys.InvalidUser(Users.Xilapa.Id);

        // Act
        var result = await PostAsync("user/logout-all-devices");

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Verifying on cache that there is no whitelisted token
        var cacheEntry = await FakeCache.GetAsync<string[]>(cachekey);
        cacheEntry.Should().BeEmpty();

        // Verifying that the correct expiration was set on cache
        FakeCache.Cache[cachekey]
            .Expiration.TotalSeconds
            .Should().Be(_loginTokenExpiration);

        // Verifying that the token does not work anymore
        var newResult = await PostAsync("user/logout-all-devices");

        newResult.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReactivateAccountEmailIsSentCorrectly()
    {
        // Arrange
        EmailServiceMock.Invocations.Clear();

        // Ensuring that the user is deactivated
        await AppFactory.WithDbContext(ctx =>
            ctx.Database.ExecuteSqlRawAsync(@$"UPDATE Users 
                                                SET Active = 0,
                                                    SecurityStamp = NULL
                                                WHERE Id = '{Users.Xilapa.Id}'"));

        var command = new SendReactivateAccountEmailCommand {UserId = Users.Xilapa.Id};

        // Act
        var result = await PutAsync("user/send-reactivate-account-email", command);

        // Await fire and forget to execute
        await Task.Delay(300);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.SecurityStamp.Should().NotBeNull();
        userFromDb.Active.Should().BeFalse();
        userFromDb.LastUpdatedAt.Should().Be(CurrentTime);

        // Verifying on cache
        FakeCache.Cache[userFromDb.SecurityStamp!]
            .Value
            .Should().Be(userFromDb.Id.ToString());

        FakeCache.Cache[userFromDb.SecurityStamp!]
            .Expiration.TotalSeconds
            .Should().Be(_accountReactivationTokenExpiration);

        // Verifying that email was sent only once
        EmailServiceMock.Verify(e =>
                e.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verifying that the correct message was sent
        var link = $"{TestSettings.FrontEndUrl}/#/security/reactivate-account?t={userFromDb.SecurityStamp}";
        var message =
            MailMessageGenerator.AccountActivation(userFromDb.Name, userFromDb.Email, link,
                userFromDb.Language);

        (EmailServiceMock.Invocations[0].Arguments[0] as MailMessage)
            .Should().BeEquivalentTo(message);

        // Finalize
        await ResetTestData();
    }

    [Fact]
    public async Task AccountIsDeactivated()
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Ensuring that the user is active
        await AppFactory.WithDbContext(ctx =>
            ctx.Database.ExecuteSqlRawAsync(@$"UPDATE Users 
                                                SET Active = 1,
                                                    SecurityStamp = NULL
                                                WHERE Id = '{Users.Xilapa.Id}'"));

        // Act
        var result = await PutAsync("user/deactivate");

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.Active.Should().BeFalse();
        userFromDb.LastUpdatedAt.Should().Be(CurrentTime);

        // Finalize
        await ResetTestData();
    }

    [Fact]
    public async Task AccountIsReactivatedWithValidToken()
    {
        // Arrange
        FakeCache.Cache.Clear();
        const string fakeToken = "TEST_TOKEN_USER_REACTIVATION";

        // Ensuring that the user is deactivated
        await AppFactory.WithDbContext(ctx =>
            ctx.Database.ExecuteSqlRawAsync(@$"UPDATE Users 
                                                SET Active = 0,
                                                SecurityStamp = '{fakeToken}'
                                                WHERE Id = '{Users.Xilapa.Id}'"));

        // Setting a fake token on cache
        FakeCache.Cache[fakeToken] = new FakeCacheEntry(Users.Xilapa.Id, TimeSpan.Zero);
        var reactivateAccountCommand = new ReactivateAccountCommand {Token = fakeToken};

        // Act
        var result = await PutAsync("user/reactivate-account", reactivateAccountCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);

        // Checking that the user was reactivated
        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.Active.Should().BeTrue();
        userFromDb.SecurityStamp.Should().BeNull();
        userFromDb.LastUpdatedAt.Should().Be(CurrentTime);

        // Verifying on cache that the token was removed
        FakeCache.Cache.Should().BeEmpty();

        // Finalize
        await ResetTestData();
    }

    [Fact]
    public async Task AccountIsNotReactivatedWithInvalidValidToken()
    {
        // Arrange
        const string fakeToken = "TEST_TOKEN_USER_REACTIVATION";

        // Ensuring that the user is deactivated
        await AppFactory.WithDbContext(ctx =>
            ctx.Database.ExecuteSqlRawAsync(@$"UPDATE Users 
                                                SET Active = 0,
                                                SecurityStamp = '{fakeToken}'
                                                WHERE Id = '{Users.Xilapa.Id}'"));

        // Setting a fake token on cache
        var fakeTokenEntry = new FakeCacheEntry(Users.Xilapa.Id, TimeSpan.Zero);
        FakeCache.Cache[fakeToken] = fakeTokenEntry;
        var reactivateAccountCommand = new ReactivateAccountCommand {Token = "INVALID_TOKEN"};

        // Act
        var result = await PutAsync("user/reactivate-account", reactivateAccountCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        var typedResult = result.GetTyped<string[]>();
        typedResult!.Length.Should().Be(1);
        typedResult[0].Should().Be(ApplicationErrors.ValueIsInvalid(nameof(ConfirmEmailCommand.Token)));

        // Checking that the user was reactivated
        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.Active.Should().BeFalse();

        // Verifying on cache that the token was not removed
        FakeCache.Cache[fakeToken]
            .Should().Be(fakeTokenEntry);

        // Finalize
        await ResetTestData();
    }

    private async Task ResetTestData()
    {
        await AppFactory.WithDbContext(async ctx =>
        {
            var user = await ctx.Users.SingleOrDefaultAsync(u => u.Id == Users.Xilapa.Id);
            if (user is not null)
                ctx.Users.Remove(user);
            ctx.Users.Add(_userXilapaWithoutChanges);
            await ctx.SaveChangesAsync();
        });

        var userReseted = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userReseted.Should().BeEquivalentTo(_userXilapaWithoutChanges);
    }
}