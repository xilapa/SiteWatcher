using FluentAssertions;
using IntegrationTests.Setup;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Emails.Messages;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.EmailTests;

public sealed class EmailSendingBaseTests : BaseTestFixture
{
    internal ITestHarness TestHarness = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        TestHarness = AppFactory.Services.GetTestHarness();
    }

    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableMasstransitTestHarness();
    }
}

public sealed class EmailSendingTest : BaseTest, IClassFixture<EmailSendingBaseTests>, IAsyncLifetime
{
    private readonly EmailSendingBaseTests _fixture;

    public EmailSendingTest(EmailSendingBaseTests fixture) : base(fixture)
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
    public async Task SendEmailOnEmailCreated()
    {
        // Arrange
        var mailRecipient = new MailRecipient(Users.Xilapa.Name, Users.Xilapa.Email, Users.Xilapa.Id);
        var (email, emailCreatedMessage) = Email.CreateEmail("body", true, "subject", mailRecipient, CurrentTime);

        // Act
        await AppFactory.WithServiceProvider(async sp =>
        {
            var context = sp.GetRequiredService<ISiteWatcherContext>();
            context.Emails.Add(email);
            var publisher = sp.GetRequiredService<IPublisher>();
            await publisher.PublishAsync(emailCreatedMessage, CancellationToken.None);
            await context.SaveChangesAsync(CancellationToken.None);
        });

        await _fixture.TestHarness.InactivityTask; // await until all messages are consumed

        // Assert
        (await _fixture.TestHarness.Published.Any<EmailCreatedMessage>()).Should().BeTrue();
        (await _fixture.TestHarness.Consumed.Any<EmailCreatedMessage>()).Should().BeTrue();

        // Verifying that the message was published only one time
        var emailCreatedMessagePublished = _fixture.TestHarness.Published
            .Select<EmailCreatedMessage>().Single().MessageObject as EmailCreatedMessage;
        emailCreatedMessagePublished.Should().BeEquivalentTo(
            new EmailCreatedMessage
            {
                Body = "body", HtmlBody = true, Subject = "subject",
                Recipients = new []{mailRecipient}, EmailId = email.Id
            }, opt => opt.Excluding(_ => _.Id));

        // Verifying that the email service was called only one time
        EmailServiceMock
            .Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<MailRecipient[]>(), It.IsAny<CancellationToken>()), Times.Once);
        (EmailServiceMock.Invocations.Single().Arguments[2] as MailRecipient[])![0]
            .Should().BeEquivalentTo(mailRecipient);
    }
}