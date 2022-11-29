using SiteWatcher.Domain.Emails;

namespace SiteWatcher.Common.Services;

public interface IEmailService
{
    Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken);
}