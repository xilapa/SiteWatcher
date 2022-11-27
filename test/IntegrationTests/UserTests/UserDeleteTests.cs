using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Moq;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Users;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.UserTests;

public class UserDeleteTestsBase : BaseTestFixture
{
    public override Action<CustomWebApplicationOptions>? Options => opts =>
        opts.DatabaseType = DatabaseType.SqliteOnDisk;
}

public class UserDeleteTests : BaseTest, IClassFixture<UserDeleteTestsBase>, IAsyncLifetime
{
    private User _userXilapaWithoutChanges;

    public UserDeleteTests(UserDeleteTestsBase fixture) : base(fixture)
    { }

    public async Task InitializeAsync()
    {
        _userXilapaWithoutChanges = await AppFactory.WithDbContext(ctx
            => ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));
    }

    public Task DisposeAsync() =>
        Task.CompletedTask;

    [Fact]
    public async Task UserIsDeleted()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        EmailServiceMock.Invocations.Clear();

        // Verifying that the user exists
        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));
        userFromDb.Should().NotBeNull();

        // Act
        var result = await DeleteAsync("user");

        // Await fire and forget to execute
        await Task.Delay(300);

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Verifying that the user was deleted
        (await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleOrDefaultAsync(u => u.Id == Users.Xilapa.Id)))
            .Should().BeNull();

        // Verifying that the email was sent
        EmailServiceMock.Verify(e =>
                e.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verifying that the correct message was sent
        var message =
            MailMessageGenerator.AccountDeleted(userFromDb.Name, userFromDb.Email, userFromDb.Language);

        (EmailServiceMock.Invocations[0].Arguments[0] as MailMessage)
            .Should().BeEquivalentTo(message);

        // Finalize
        await ResetTestData();
    }

    [Fact]
    public async Task UserDeletedEmailIsNotSentIfUserIsAlreadyDeleted()
    {
        // Arrange
        LoginAs(Users.Xilapa);
        EmailServiceMock.Invocations.Clear();

        // Ensuring that user does not exists
        await AppFactory.WithDbContext(async ctx =>
        {
            var user = await ctx.Users.SingleOrDefaultAsync(u => u.Id == Users.Xilapa.Id);
            if (user is not null)
                ctx.Users.Remove(user);
            await ctx.SaveChangesAsync();
        });

        // Act
        var result = await DeleteAsync("user");

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Verifying that the email was not sent
        EmailServiceMock.Verify(e =>
                e.SendEmailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);

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