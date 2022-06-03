using MimeKit;
using SiteWatcher.Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit.Text;
using SiteWatcher.Domain.Models.Email;

namespace SiteWatcher.Infra.Email;

public class EmailService : IEmailService
{
    private readonly IEmailSettings _emailSettings;

    public EmailService(IEmailSettings emailSettings)
    {
        _emailSettings = emailSettings;
    }

    public async Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));

        foreach (var recip in mailMessage.Recipients)
            msg.To.Add(new MailboxAddress(recip.Name, recip.Email));

        msg.Subject = mailMessage.Subject;
        msg.Body = new TextPart(mailMessage.HtmlBody ? TextFormat.Html : TextFormat.Text) { Text = mailMessage.Body };

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.TLSPort, true, cancellationToken);
        await smtpClient.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword, cancellationToken);
        await smtpClient.SendAsync(msg, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}
