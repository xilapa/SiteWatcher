using SiteWatcher.Domain.Models.Emails;

namespace SiteWatcher.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken);
}