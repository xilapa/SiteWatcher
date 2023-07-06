using System.Net;
using System.Reflection;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.IntegrationTests.Utils;
using System.Runtime.CompilerServices;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Users.Commands.ActivateAccount;
using SiteWatcher.Application.Users.Commands.ConfirmEmail;
using SiteWatcher.Application.Users.Commands.ReactivateAccount;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Users.Messages;
using SiteWatcher.Infra.Persistence;

namespace IntegrationTests.UserTests;

public class UserTesstBase : BaseTestFixture
{
    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseDatabase(DatabaseType.SqliteOnDisk);
    }
}

public class UserTests : BaseTest, IClassFixture<UserTesstBase>, IAsyncLifetime
{
    private User _userXilapaWithoutChanges = null!;
    private int _loginTokenExpiration;

    public UserTests(UserTesstBase fixture) : base(fixture)
    {
        // Clear cache before each test to avoid Forbiden errors
        FakeCache.Cache.Clear();
        FakePublisher.Messages.Clear();
    }

    public async Task InitializeAsync()
    {
        _userXilapaWithoutChanges = await AppFactory.WithDbContext(ctx
            => ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        var authServiceInstance = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));

        _loginTokenExpiration = (int) typeof(AuthService)
            .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(f => f.Name == "LoginTokenExpiration")
            .GetValue(authServiceInstance)!;
    }

    public Task DisposeAsync() =>
        Task.CompletedTask;

    [Fact]
    public async Task GetUserInfo()
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Act
        var result = await GetAsync("user");

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var typedResult = result.GetTyped<UserViewModel>();
        typedResult.Should().BeEquivalentTo(_userXilapaWithoutChanges, o => o.ExcludingMissingMembers());
    }

    [Fact]
    public async Task UserDataIsUpdated()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        var cacheKey = $"{CacheKeys.UserInfo(Users.Xilapa.Id)}:";

        // user info should not be in cache
        FakeCache.Cache.TryGetValue(cacheKey, out _)
            .Should()
            .BeFalse();

        await GetAsync("user");

        // user info should be in cache
        FakeCache.Cache.TryGetValue(cacheKey, out _)
            .Should()
            .BeTrue();

        var updateUserCommand = new UpdateUserCommand
        {
            Email = "newemail@email.com",
            Language = Language.Spanish,
            Name = "XilapaNewName",
            Theme = Theme.Light
        };

        // Act
        var result = await PutAsync("user", updateUserCommand);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var typedResult = result.GetTyped<UpdateUserResult>();
        typedResult!
            .User
            .Should()
            .BeEquivalentTo(updateUserCommand, o => o.ExcludingMissingMembers());

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
            Language = Language.Spanish,
            Name = "Xilapa4NewName",
            Theme = Theme.Light
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
    public async Task UserEmailChangeTriggersEmailValidation()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        EmailServiceMock.Invocations.Clear();

        var updateUserCommand = new UpdateUserCommand
        {
            Email = "newemail@email.com",
            Language = Language.Spanish,
            Name = "XilapaNewName",
            Theme = Theme.Light
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

        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.EmailConfirmed.Should().BeFalse();
        userFromDb.LastUpdatedAt.Should().Be(CurrentTime);

        // Verifying that the message was sent
        var message = FakePublisher.Messages[0].Content as EmailConfirmationTokenGeneratedMessage;
        var expected = new EmailConfirmationTokenGeneratedMessage(userFromDb, CurrentTime);
        message.Should().BeEquivalentTo(expected);

        // Finalize
        await ResetTestData();
    }

    [Fact]
    public async Task EmailConfirmationTriggersEmailValidation()
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
            Language = Language.BrazilianPortuguese,
            Name = userViewModel.Name,
            Theme = Theme.Light
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

        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.GoogleId == userViewModel.GetGoogleId()));

        userFromDb.EmailConfirmed.Should().BeFalse();

        // Verifying that the message was sent
        var message = FakePublisher.Messages[0].Content as EmailConfirmationTokenGeneratedMessage;
        var expected = new EmailConfirmationTokenGeneratedMessage(userFromDb, CurrentTime);
        message.Should().BeEquivalentTo(expected);
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
    public async Task ManualEmailConfirmationWorks()
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

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Verifying that the user was updated on database
        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xulipa.Id));

        userFromDb.SecurityStamp.Should().NotBeNull();
        userFromDb.LastUpdatedAt.Should().Be(CurrentTime);

        // Verifying that the message was published
        var message = FakePublisher.Messages[0].Content as EmailConfirmationTokenGeneratedMessage;
        var expected = new EmailConfirmationTokenGeneratedMessage(userFromDb, CurrentTime);
        message.Should().BeEquivalentTo(expected);
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
    public async Task ReactivateAccountTokenMessageIsGenerated()
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

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));

        userFromDb.SecurityStamp.Should().NotBeNull();
        userFromDb.Active.Should().BeFalse();
        userFromDb.LastUpdatedAt.Should().Be(CurrentTime);

        // Verifying that the message was sent
        var message = FakePublisher.Messages[0].Content as UserReactivationTokenGeneratedMessage;
        var expected = new UserReactivationTokenGeneratedMessage(userFromDb, CurrentTime);
        message.Should().BeEquivalentTo(expected);

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