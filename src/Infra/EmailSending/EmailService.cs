using MimeKit;
using MailKit.Net.Smtp;
using MimeKit.Text;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Emails.DTOs;

namespace SiteWatcher.Infra.EmailSending;

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ISmtpClient _smtpClient;
    private readonly EmailThrottler _emailThrottler;

    public EmailService(EmailSettings emailSettings, ISmtpClient smtpClient, EmailThrottler emailThrottler)
    {
        _emailSettings = emailSettings;
        _smtpClient = smtpClient;
        _emailThrottler = emailThrottler;
    }

    public async Task SendEmailAsync(string subject, string body, MailRecipient[] recipients, CancellationToken cancellationToken)
    {
        await _emailThrottler.WaitToSend(cancellationToken);

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));

        foreach (var recip in recipients)
            msg.To.Add(new MailboxAddress(recip.Name, recip.Email));

        msg.Subject = subject;
        msg.Body = new TextPart(TextFormat.Html) { Text = body };
        msg.ReplyTo.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));

        await ConnectSmtpClient(cancellationToken);
        await _smtpClient.SendAsync(msg, cancellationToken);

        await _smtpClient.DisconnectAsync(true, CancellationToken.None);
        _smtpClient.Dispose();
    }

    private async Task ConnectSmtpClient(CancellationToken cancellationToken)
    {
        if (!_smtpClient.IsConnected)
            await _smtpClient.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.Port, _emailSettings.UseSsl, cancellationToken);

        if (!_smtpClient.IsAuthenticated)
            await _smtpClient.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword, cancellationToken);
    }
}