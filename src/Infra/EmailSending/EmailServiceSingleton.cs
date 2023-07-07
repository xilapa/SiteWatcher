using System.Threading.Channels;
using MimeKit;
using SiteWatcher.Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit.Text;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Emails.DTOs;

namespace SiteWatcher.Infra.EmailSending;

public sealed class EmailServiceSingleton : IEmailServiceSingleton
{
    private SmtpClient? _smtpClient;
    private readonly IEmailSettings _emailSettings;
    private readonly Channel<bool> _rateLimiter;

    public EmailServiceSingleton(IEmailSettings emailSettings)
    {
        _emailSettings = emailSettings;
        _rateLimiter = Channel.CreateBounded<bool>(1);
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(emailSettings.EmailDelaySeconds));
                _ = await _rateLimiter.Reader.ReadAsync();
            }
        });
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
            await _rateLimiter.Writer.WriteAsync(true, cancellationToken);
            return null;
        }
        catch (Exception e)
        {
            // If there is an error, try to disconnect,
            // set the smtp client to null and return the error
            await DisconnectSmtpClient();

            return string.IsNullOrEmpty(e.Message) ?
                $"Error sending the email: {e.InnerException?.Message ?? "No inner exception message"}"
                : $"Error sending the email: {e.Message}";
        }
    }

    private async Task<SmtpClient> GetSmtpClient(CancellationToken cancellationToken)
    {
        _smtpClient = await CheckCurrentSmtpClient();

        if (!_smtpClient.IsConnected)
            await _smtpClient.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.Port, _emailSettings.UseSsl, cancellationToken);

        if (!_smtpClient.IsAuthenticated)
            await _smtpClient.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword, cancellationToken);

        return _smtpClient;
    }

    private async Task<SmtpClient> CheckCurrentSmtpClient()
    {
        if (_smtpClient == null) return new SmtpClient();

        try
        {
            // Noop to check if the connection has timeout
            await _smtpClient.NoOpAsync();
        }
        catch
        {
            await DisconnectSmtpClient();
            return new SmtpClient();
        }

        return _smtpClient;
    }

    private async Task DisconnectSmtpClient()
    {
        if(_smtpClient == null)
            return;
        try
        {
            await _smtpClient.DisconnectAsync(quit: true);
            _smtpClient.Dispose();
        }
        finally
        {
            _smtpClient = null;
        }
    }
}