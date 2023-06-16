using SiteWatcher.Domain.Emails.DTOs;

namespace SiteWatcher.Common.Services;

public interface IEmailService
{
    Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken);
}