using SiteWatcher.Domain.Emails;

namespace SiteWatcher.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken);
}