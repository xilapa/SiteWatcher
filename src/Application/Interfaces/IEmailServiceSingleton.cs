using SiteWatcher.Domain.Emails;

namespace SiteWatcher.Application.Interfaces;

public interface IEmailServiceSingleton
{
    /// <summary>
    /// Send the email and return the error message, if any.
    /// </summary>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="recipients">Email recipients</param>
    /// <param name="cancellationToken">Cancelation Token</param>
    /// <returns></returns>
    Task<string?> SendEmailAsync(string subject, string body, MailRecipient[] recipients, CancellationToken cancellationToken);
}