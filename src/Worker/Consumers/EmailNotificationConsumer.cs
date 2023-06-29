using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Infra;
using SiteWatcher.Worker.Persistence;

namespace SiteWatcher.Worker.Consumers;

public sealed class EmailNotificationConsumer : IEmailNotificationConsumer, ICapSubscribe
{
    private readonly ILogger<EmailNotificationConsumer> _logger;
    private readonly SiteWatcherContext _context;
    private readonly IEmailServiceSingleton _emailService;
    private readonly IEmailSettings _emailSettings;

    public EmailNotificationConsumer(ILogger<EmailNotificationConsumer> logger, SiteWatcherContext context,
     IEmailServiceSingleton emailService, IEmailSettings emailSettings)
    {
        _logger = logger;
        _context = context;
        _emailService = emailService;
        _emailSettings = emailSettings;
    }

    // CAP uses this attribute to create a queue and bind it with a routing key.
    // The message name is the routing key and group name is used to create the queue name.
    // Cap append the version on the queue name (e.g., queue-name.v1)
    [CapSubscribe(RoutingKeys.MailMessage, Group = RoutingKeys.MailMessage)]
    public async Task Consume(MailMessage message, CancellationToken cancellationToken)
    {
        var messageId = message.EmailId.ToString();
        var hasBeenProcessed = await _context.HasBeenProcessed(messageId!, nameof(EmailNotificationConsumer));
        if (hasBeenProcessed)
        {
            _logger.LogInformation("{Date} MailMessage has already been processed: {MessageId}", DateTime.UtcNow, message.EmailId);
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        /// TODO: move inner email sending logic to Application UseCase, and set error or mark email as sent
        var error = await _emailService.SendEmailAsync(message.Subject, message.Body!, message.Recipients, cancellationToken);

        if (error != null)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(_emailSettings.EmailDelaySeconds), cancellationToken);
            // If there is an error, throw an exception to fail the message consuming
            throw new Exception(error);
        }

        _context.MarkMessageAsConsumed(messageId!, nameof(EmailNotificationConsumer));
        await _context.SaveChangesAsync(cancellationToken);

        await Task.Delay(TimeSpan.FromSeconds(_emailSettings.EmailDelaySeconds), cancellationToken);
        _logger.LogInformation("{Date} MailMessage consumed: {MessageId}", DateTime.UtcNow, messageId);
    }
}

public interface IEmailNotificationConsumer
{
    Task Consume(MailMessage message, CancellationToken cancellationToken);
}