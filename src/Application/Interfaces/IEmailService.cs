using SiteWatcher.Domain.Models.Email;

namespace SiteWatcher.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken);
}