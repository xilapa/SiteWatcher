using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Moq;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Emails.Events;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.UserTests;

public class UserDeleteTestsBase : BaseTestFixture
{
    protected override void OnConfiguringTestServer(CustomWebApplicationOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseDatabase(DatabaseType.SqliteOnDisk);
    }
}

public class UserDeleteTests : BaseTest, IClassFixture<UserDeleteTestsBase>, IAsyncLifetime
{
    private User _userXilapaWithoutChanges;

    public UserDeleteTests(UserDeleteTestsBase fixture) : base(fixture)
    {
        FakePublisher.Messages.Clear();
    }

    public async Task InitializeAsync()
    {
        _userXilapaWithoutChanges = await AppFactory.WithDbContext(ctx
            => ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));
    }

    public Task DisposeAsync() =>
        Task.CompletedTask;

    [Theory]
    [InlineData(Language.English)]
    [InlineData(Language.BrazilianPortuguese)]
    [InlineData(Language.Spanish)]
    public async Task UserIsDeleted(Language language)
    {
        // Arrange
        await UpdateUsersLanguage(language);
        LoginAs(Users.Xilapa);

        // Verifying that the user exists
        var userFromDb = await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleAsync(u => u.Id == Users.Xilapa.Id));
        userFromDb.Should().NotBeNull();

        var expectedBody = LocalizedMessages.AccountDeletedBody(userFromDb.Language);
        var expectedSubject = LocalizedMessages.AccountDeletedSubject(userFromDb.Language);

        // Act
        var result = await DeleteAsync("user");

        // Assert
        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        // Verifying that the user was deleted
        (await AppFactory.WithDbContext(ctx =>
            ctx.Users.SingleOrDefaultAsync(u => u.Id == Users.Xilapa.Id)))
            .Should().BeNull();

        // Verifying that the email message was published
        var emailMessage = FakePublisher.Messages[0].Content as EmailCreatedMessage;
        emailMessage!.Body.Should().Be(expectedBody);
        emailMessage.Subject.Should().Be(expectedSubject);
        emailMessage.EmailId.Should().NotBeNull();
        emailMessage.Recipients.Should()
            .BeEquivalentTo(new []{new MailRecipient(userFromDb.Name, userFromDb.Email, userFromDb.Id)});

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
        EmailServiceMock
            .Verify(e =>
                    e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MailRecipient[]>(),
                        It.IsAny<CancellationToken>()),
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