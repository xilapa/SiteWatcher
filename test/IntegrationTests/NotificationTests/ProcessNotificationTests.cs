using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Entities.Triggerings;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.Messages;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Emails.Messages;
using SiteWatcher.Domain.Notifications;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.NotificationTests;

public sealed class ProcessNotificationTestBase : BaseTestFixture
{
    internal Alert Alert = null!;
    internal AlertTriggered[] AlertTriggereds = null!;
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Alert = await AppFactory.CreateAlert("alert to notify", Rules.Regex, Users.Xilapa.Id);
        AlertTriggereds = new[]
        {
            new AlertTriggered(Alert, TriggeringStatus.Success, AppFactory.CurrentTime)
        };
    }

    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddMessageHandlers();
    }
}

public sealed class ProcessNotificationTests : BaseTest, IClassFixture<ProcessNotificationTestBase>
{
    private readonly ProcessNotificationTestBase _fixture;

    public ProcessNotificationTests(ProcessNotificationTestBase fixture) : base(fixture)
    {
        _fixture = fixture;
        FakePublisher.Messages.Clear();
    }

    [Theory]
    [InlineData(Language.English, "have been triggered", "couldn't be reached")]
    [InlineData(Language.Spanish, "Se han activado", "No se pudo acceder")]
    [InlineData(Language.BrazilianPortuguese, "foram disparados", "Não foi possível acessar")]
    public async Task ProcessNotificationTest(Language language, string expected, string notExpected)
    {
        // Arrange
        await ClearNotifications();
        await UpdateUsersLanguage(language);
        var user = await GetUser();
        CurrentTime = CurrentTime.AddSeconds(1); // Change time to avoid conflict on messageId

        var message = new AlertsTriggeredMessage(user, _fixture.AlertTriggereds, AppFactory.CurrentTime);

        // Act
        await ProcessNotification(message);

        // Assert

        // Check the notification created
        var notification = await GetNotification();
        notification.Id.Should().NotBeNull().And.NotBe(NotificationId.Empty);
        notification.UserId.Should().Be(user.Id);
        notification.CreatedAt.Should().Be(AppFactory.CurrentTime);
        notification.EmailId.Should().NotBeNull().And.NotBe(EmailId.Empty);
        notification.Alerts.Should().HaveCount(1);
        notification.Alerts.First().Id.Should().Be(_fixture.Alert.Id);
        notification.NotificationAlerts
            .Should()
            .BeEquivalentTo(new[]
            {
                new NotificationAlert(notification.Id, _fixture.AlertTriggereds[0])
            });

        // Check the email created
        notification.Email.UserId.Should().Be(user.Id);
        notification.Email.Recipient.Should().Be($"{user.Name}:{user.Email}");
        var rule = _fixture.AlertTriggereds[0].Rule;
        AssertEmailBody(notification.Email.Body, language, rule , expected, notExpected);

        // Check the message published
        var mailMessage = (FakePublisher.Messages.Single().Content as EmailCreatedMessage)!;
        mailMessage.Recipients.Should().BeEquivalentTo(new[]
        {
            new MailRecipient(user.Name, user.Email, user.Id)
        });
        mailMessage.HtmlBody.Should().BeTrue();
        mailMessage.EmailId.Should().Be(notification.Email.Id);
        AssertEmailBody(mailMessage.Body!, language, rule, expected, notExpected);
    }

    private async Task ClearNotifications()
    {
        await AppFactory.WithDbContext(async ctx =>
        {
            await ctx.Notifications.ExecuteDeleteAsync();
            await ctx.Emails.ExecuteDeleteAsync();
            await ctx.Set<NotificationAlert>().ExecuteDeleteAsync();
        });
    }

    private Task<User> GetUser()
    {
        return AppFactory.WithDbContext(ctx =>
            ctx.Users.Where(u => u.Id == Users.Xilapa.Id).SingleAsync());
    }

    private async Task ProcessNotification(AlertsTriggeredMessage message)
    {
        await AppFactory.WithServiceProvider(async sp =>
        {
            var handler = sp.GetRequiredService<IMessageHandler<AlertsTriggeredMessage>>();
            await handler.Handle(message, CancellationToken.None);
        });
    }

    private Task<Notification> GetNotification()
    {
        return AppFactory.WithDbContext(ctx =>
            ctx.Notifications
                .Include(n => n.Alerts)
                .Include(n => n.Email)
                .Include(n => n.NotificationAlerts)
                .AsNoTracking()
                .SingleAsync());
    }

    private void AssertEmailBody(string body, Language lang, Rules rule, string expected,
        string notExpected)
    {
        body.Should().NotContain(notExpected);
        body.Should().Contain(expected);
        body.Should().Contain(_fixture.Alert.Name);
        body.Should().Contain(_fixture.Alert.Site.Name);
        body.Should().Contain(_fixture.Alert.Site.Uri.ToString());
        body.Should()
            .Contain(LocalizedMessages.FrequencyString(lang, _fixture.Alert.Frequency));
        body.Should()
            .Contain(LocalizedMessages.RuleString(lang, rule));
    }
}
