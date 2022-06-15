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
using SiteWatcher.Infra.Authorization;

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

        // Verifing on cache
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

        // Verifing that the correct message was sent
        var link = $"{TestSettings.FrontEndUrl}/#/security/confirm-email?t={userFromDb.SecurityStamp}";
        var message =
            MailMessageGenerator.EmailConfirmation(updateUserCommand.Name, updateUserCommand.Email, link,
                updateUserCommand.Language);

        (EmailServiceMock.Invocations[0].Arguments[0] as MailMessage)
            .Should().BeEquivalentTo(message);

        // Finalize
        await ResetTestData();
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