using FluentAssertions;
using MailKit.Net.Smtp;
using MimeKit;
using NSubstitute;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Infra.EmailSending;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Utils;

namespace UnitTests.Services;

public sealed class EmailServiceTests
{
    private readonly EmailService _emailService;
    private readonly ISmtpClient _smtpClient;
    private readonly EmailSettings _emailSettings;

    public EmailServiceTests()
    {
        _emailSettings = new EmailSettings
        {
            FromName = "TestApp",
            FromEmail = "testapp@test.com",
            SmtpHost = "testHost",
            Port = 007,
            UseSsl = true,
            SmtpUser = "testUser",
            SmtpPassword = "strongPassword",
            EmailDelaySeconds = 20
        };
        _smtpClient = Substitute.For<ISmtpClient>();
        var testSession = new TestSession(null!, DateTime.UtcNow);
        var emailThrottler = new EmailThrottler(_emailSettings, testSession);
        _emailService = new EmailService(_emailSettings, _smtpClient, emailThrottler);
    }

    [Fact]
    public async Task EmailIsSentCorrectly()
    {
        // Arrange
        const string subject = "Email Subject";
        const string body = "Hello!";
        var recipients = new MailRecipient[]
        {
            new("Xilapa", "xilapa@email.com", Users.Xilapa.Id),
            new("Xulipa", "xulipa@email.com", Users.Xulipa.Id),
        };

        MimeMessage? messageSent = default;
        await _smtpClient.SendAsync(Arg.Do<MimeMessage>(m => messageSent = m));

        // Act
        await _emailService.SendEmailAsync(subject, body, recipients, CancellationToken.None);

        // Assert
        await _smtpClient
            .Received(1)
            .ConnectAsync(_emailSettings.SmtpHost, _emailSettings.Port, _emailSettings.UseSsl, CancellationToken.None);

        await _smtpClient
            .Received(1)
            .AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword, CancellationToken.None);

        await _smtpClient
            .Received(1)
            .SendAsync(Arg.Any<MimeMessage>(), CancellationToken.None);

        ValidateEmailSent(messageSent!, recipients, subject, body);

        await _smtpClient
            .Received(1)
            .DisconnectAsync(true, CancellationToken.None);

        _smtpClient
            .Received(1)
            .Dispose();
    }

    private void ValidateEmailSent(MimeMessage messageSent, MailRecipient[] recipients, string subject, string body)
    {
        messageSent!.From.Mailboxes.Single().Name.Should().Be(_emailSettings.FromName);
        messageSent.From.Mailboxes.Single().Address.Should().Be(_emailSettings.FromEmail);
        messageSent.To.Mailboxes
            .Should()
            .Contain(e => e.Name == recipients[0].Name && e.Address == recipients[0].Email)
            .And
            .Contain(e => e.Name == recipients[1].Name && e.Address == recipients[1].Email);
        messageSent.Subject.Should().Be(subject);
        messageSent.Body.ContentType.MimeType.Should().Be("text/html");
        messageSent.Body.ToString().Should().Contain(body);
        messageSent.ReplyTo.Mailboxes.Single().Name.Should().Be(_emailSettings.FromName);
        messageSent.ReplyTo.Mailboxes.Single().Address.Should().Be(_emailSettings.FromEmail);
    }
}