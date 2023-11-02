using MimeKit;
using MailKit.Net.Smtp;
using MimeKit.Text;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Emails.DTOs;

namespace SiteWatcher.Infra.EmailSending;

public sealed class EmailServiceSingleton : IEmailServiceSingleton
{
    private SmtpClient? _smtpClient;
    private readonly EmailSettings _emailSettings;
    private readonly ISession _session;
    private DateTime? _lastEmailSentDate;
    private readonly TimeSpan _emailDelay;

    public EmailServiceSingleton(EmailSettings emailSettings, ISession session)
    {
        _emailSettings = emailSettings;
        _session = session;
        _emailDelay = TimeSpan.FromSeconds(_emailSettings.EmailDelaySeconds);
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
            var timeSinceLastEmail = _session.Now - _lastEmailSentDate;
            if (_lastEmailSentDate.HasValue && timeSinceLastEmail < _emailDelay)
                await  Task.Delay(_emailDelay - timeSinceLastEmail.Value, cancellationToken);

            var smtpClient = await GetSmtpClient(cancellationToken);
            await smtpClient.SendAsync(msg, cancellationToken);

            _lastEmailSentDate = _session.Now;
            return null;
        }
        catch (Exception e)
        {
            // If there is an error, try to disconnect,
            // set the smtp client to null and return the error
            await DisconnectSmtpClient(cancellationToken);

            return string.IsNullOrEmpty(e.Message) ?
                $"Error sending the email: {e.InnerException?.Message ?? "No inner exception message"}"
                : $"Error sending the email: {e.Message}";
        }
    }

    private async Task<SmtpClient> GetSmtpClient(CancellationToken cancellationToken)
    {
        _smtpClient = await CheckCurrentSmtpClient(cancellationToken);

        if (!_smtpClient.IsConnected)
            await _smtpClient.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.Port, _emailSettings.UseSsl, cancellationToken);

        if (!_smtpClient.IsAuthenticated)
            await _smtpClient.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword, cancellationToken);

        return _smtpClient;
    }

    private async Task<SmtpClient> CheckCurrentSmtpClient(CancellationToken cancellationToken)
    {
        if (_smtpClient == null) return new SmtpClient();

        try
        {
            // Noop to check if the connection has timeout
            await _smtpClient.NoOpAsync(cancellationToken);
        }
        catch
        {
            await DisconnectSmtpClient(cancellationToken);
            return new SmtpClient();
        }

        return _smtpClient;
    }

    private async Task DisconnectSmtpClient(CancellationToken cancellationToken)
    {
        if(_smtpClient == null) return;
        try
        {
            await _smtpClient.DisconnectAsync(quit: true, cancellationToken);
        }
        finally
        {
            DisposeSmtpClient();
        }
    }

    private void DisposeSmtpClient()
    {
        if(_smtpClient == null) return;
        try
        {
            _smtpClient.Dispose();
        }
        finally
        {
            _smtpClient = null;
        }
    }
}