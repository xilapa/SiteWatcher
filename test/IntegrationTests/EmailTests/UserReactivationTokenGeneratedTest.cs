using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using IntegrationTests.Setup;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Emails.Messages;
using SiteWatcher.Domain.Users.Messages;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.EmailTests;

public sealed class UserReactivationTokenGeneratedTestBase : BaseTestFixture
{
    internal ITestHarness TestHarness = null!;
    internal int AccountReactivationTokenExpiration;

    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableMessageConsumers();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        // TestHarness should be used only once per test class
        // https://masstransit.io/documentation/concepts/testing#test-harness-concepts
        TestHarness = AppFactory.Services.GetTestHarness();

        var authServiceInstance = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));
        AccountReactivationTokenExpiration = (int)typeof(AuthService)
            .GetField("AccountReactivationTokenExpiration", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(authServiceInstance)!;
    }
}

public sealed class UserReactivationTokenGeneratedTest : BaseTest, IAsyncLifetime,
    IClassFixture<UserReactivationTokenGeneratedTestBase>
{
    private readonly UserReactivationTokenGeneratedTestBase _fixture;

    public UserReactivationTokenGeneratedTest(UserReactivationTokenGeneratedTestBase fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.TestHarness.Start();
    }

    public async Task DisposeAsync()
    {
        await _fixture.TestHarness.Stop();
    }

    [Fact]
    public async Task CreateAndSendEmailOnUserReactivationTokenGenerated()
    {
        // Arrange
        await AppFactory.WithDbContext(ctx =>
            ctx.Database.ExecuteSqlRawAsync(@$"UPDATE Users 
                                                SET Active = 0,
                                                    SecurityStamp = NULL
                                                WHERE Id = '{Users.Xilapa.Id}'"));

        // Act
        var user = await AppFactory.WithServiceProvider(async sp =>
        {
            var context = sp.GetRequiredService<ISiteWatcherContext>();
            var user = context.Users.First(u => u.Id == Users.Xilapa.Id);
            user.GenerateReactivationToken(CurrentTime);
            await context.SaveChangesAsync(CancellationToken.None);
            return user;
        });

        await _fixture.TestHarness.InactivityTask; // await until all messages are consumed

        // Assert
        (await _fixture.TestHarness.Published.Any<UserReactivationTokenGeneratedMessage>()).Should().BeTrue();
        (await _fixture.TestHarness.Consumed.Any<UserReactivationTokenGeneratedMessage>()).Should().BeTrue();

        // Verifying that the message was published only one time
        var userReactivationMessagePublished = _fixture.TestHarness.Published
            .Select<UserReactivationTokenGeneratedMessage>().Single()
            .MessageObject as UserReactivationTokenGeneratedMessage;

        userReactivationMessagePublished.Should()
            .BeEquivalentTo(new UserReactivationTokenGeneratedMessage
            {
                UserId = user.Id, Email = user.Email, Language = user.Language,
                ConfirmationToken = user.SecurityStamp!, Name = user.Name
            }, o => o.Excluding(_ => _.Id));

        // Verifying with the auth token expiration was set
        // Auth token is set as a key on cache with the user id as value
        (FakeCache.Cache[user.SecurityStamp!].Value as string)!.Should().Be(user.Id.ToString());
        FakeCache.Cache[user.SecurityStamp!].Expiration.Should()
            .Be(TimeSpan.FromSeconds(_fixture.AccountReactivationTokenExpiration));

        // An Email message should be created and consumed
        _fixture.TestHarness.Published.Select<EmailCreatedMessage>().Count().Should().Be(1);
        _fixture.TestHarness.Consumed.Select<EmailCreatedMessage>().Count().Should().Be(1);
    }
}