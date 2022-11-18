using MimeKit;
using SiteWatcher.Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit.Text;
using SiteWatcher.Domain.Models.Emails;

namespace SiteWatcher.Infra.EmailSending;

public sealed class EmailServiceSingleton : IEmailServiceSingleton
{
    private SmtpClient? _smtpClient;
    private readonly IEmailSettings _emailSettings;

    public EmailServiceSingleton(IEmailSettings emailSettings)
    {
        _emailSettings = emailSettings;
    }

    public async Task<string?> SendEmailAsync(string subject, string body, MailRecipient[] recipients, CancellationToken cancellationToken)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));

        foreach (var recip in recipients)
            msg.To.Add(new MailboxAddress(recip.Name, recip.Email));

        msg.Subject = subject;
        msg.Body = new TextPart(TextFormat.Html) { Text = body };
        msg.ReplyTo.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));

        try
        {
            var smtpClient = await GetSmtpClient(cancellationToken);
            await smtpClient.SendAsync(msg, cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(_emailSettings.EmailDelaySeconds), cancellationToken);
            return null;
        }
        catch (Exception e)
        {
            // If there is an error, try to disconnect,
            // set the smtp client to null and return the error
            await TryDisconnectClient();
            _smtpClient = null;
            return e.Message;
        }
    }

    private async Task<SmtpClient> GetSmtpClient(CancellationToken cancellationToken)
    {
        _smtpClient ??= new SmtpClient();

        if (!_smtpClient.IsConnected)
            await _smtpClient.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.Port, _emailSettings.UseSsl, cancellationToken);

        if (!_smtpClient.IsAuthenticated)
            await _smtpClient.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword, cancellationToken);

        return _smtpClient;
    }

    private async Task TryDisconnectClient()
    {
        if(_smtpClient == null)
            return;
        try
        {
            await _smtpClient.DisconnectAsync(quit: true);
        }
        catch { }
    }
}