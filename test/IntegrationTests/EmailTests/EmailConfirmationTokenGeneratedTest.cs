using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using IntegrationTests.Setup;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Emails.Messages;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Domain.Users.Messages;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;

namespace IntegrationTests.EmailTests;

public sealed class EmailConfirmationTokenGeneratedTestBase : BaseTestFixture
{
    internal ITestHarness TestHarness = null!;
    internal int EmailConfirmationTokenExpiration;

    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableMasstransitTestHarness();
        var authServiceInstance = RuntimeHelpers.GetUninitializedObject(typeof(AuthService));
        EmailConfirmationTokenExpiration = (int)typeof(AuthService)
            .GetField("EmailConfirmationTokenExpiration", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(authServiceInstance)!;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        TestHarness = AppFactory.Services.GetTestHarness();
    }
}

public sealed class EmailConfirmationTokenGeneratedTest : BaseTest, IAsyncLifetime,
    IClassFixture<EmailConfirmationTokenGeneratedTestBase>
{
    private readonly EmailConfirmationTokenGeneratedTestBase _fixture;

    public EmailConfirmationTokenGeneratedTest(EmailConfirmationTokenGeneratedTestBase fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.TestHarness.Start();
        FakeCache.Cache.Clear();
    }

    public async Task DisposeAsync()
    {
        await _fixture.TestHarness.Stop();
    }

    [Fact]
    public async Task CreateAndSendEmailOnEmailConfirmationTokenGenerated()
    {
        // Arrange
        var user = new User("googleId", "name", "email", "authEmail", Language.English,
            Theme.Light, CurrentTime);

        // Act
        await AppFactory.WithServiceProvider(async sp =>
        {
            var context = sp.GetRequiredService<ISiteWatcherContext>();
            context.Users.Add(user);
            await context.SaveChangesAsync(CancellationToken.None);
        });

        await _fixture.TestHarness.InactivityTask; // await until all messages are consumed

        // Assert
        (await _fixture.TestHarness.Published.Any<EmailConfirmationTokenGeneratedMessage>()).Should().BeTrue();
        (await _fixture.TestHarness.Consumed.Any<EmailConfirmationTokenGeneratedMessage>()).Should().BeTrue();

        // Verifying that the message was published only one time
        var emailConfirmationTokenGeneratedMessagePublished = _fixture.TestHarness.Published
            .Select<EmailConfirmationTokenGeneratedMessage>().Single()
            .MessageObject as EmailConfirmationTokenGeneratedMessage;
        emailConfirmationTokenGeneratedMessagePublished.Should()
            .BeEquivalentTo(new EmailConfirmationTokenGeneratedMessage
            {
                Email = user.Email, UserId = user.Id, Language = user.Language,
                Name = user.Name, ConfirmationToken = user.SecurityStamp!
            }, o => o.Excluding(_ => _.Id));

        // Verifying with the auth token expiration was set
        // Auth token is set as a key on cache with the user id as value
        (FakeCache.Cache[user.SecurityStamp!].Value as string)!.Should().Be(user.Id.ToString());
        FakeCache.Cache[user.SecurityStamp!].Expiration.Should()
            .Be(TimeSpan.FromSeconds(_fixture.EmailConfirmationTokenExpiration));

        // An Email message should be created and consumed
        _fixture.TestHarness.Published.Select<EmailCreatedMessage>().Count().Should().Be(1);
        _fixture.TestHarness.Consumed.Select<EmailCreatedMessage>().Count().Should().Be(1);
    }
}