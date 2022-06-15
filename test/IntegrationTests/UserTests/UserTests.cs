using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Users.Commands.UpdateUser;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace IntegrationTests.UserTests;

public class UserTests : BaseTest, IClassFixture<BaseTestFixture>, IAsyncLifetime
{
    private User _userXilapaWithoutChanges = null!;

    public UserTests(BaseTestFixture fixture) : base(fixture)
    { }

    public async Task InitializeAsync()
    {
        _userXilapaWithoutChanges = await WithDbContext(ctx
            => ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));
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