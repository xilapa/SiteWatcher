using System.Text.Json;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra;
using SiteWatcher.Worker.Messaging;
using SiteWatcher.Worker.Persistence;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Worker.Consumers;

public sealed class EmailNotificationConsumer : IEmailNotificationConsumer, ICapSubscribe
{
    private readonly ILogger<EmailNotificationConsumer> _logger;
    private readonly SiteWatcherContext _context;
    private readonly IEmailServiceSingleton _emailService;

    public EmailNotificationConsumer(ILogger<EmailNotificationConsumer> logger, SiteWatcherContext context,
     IEmailServiceSingleton emailService)
    {
        _logger = logger;
        _context = context;
        this._emailService = emailService;
    }

    // CAP uses this attribute to create a queue and bind it with a routing key.
    // The message name is the routing key and group name is used to create the queue name.
    // Cap append the version on the queue name (e.g., queue-name.v1)
    [CapSubscribe(RoutingKeys.EmailNotification, Group = RoutingKeys.EmailNotification)]
    public async Task Consume(EmailNotificationMessage message, [FromCap] CapHeader capHeader, CancellationToken cancellationToken)
    {
        var messageId = capHeader[MessageHeaders.MessageIdKey]!;
        var hasBeenProcessed = await _context.HasBeenProcessed(messageId, nameof(EmailNotificationConsumer));
        if (hasBeenProcessed)
        {
            _logger.LogInformation("{Date} Message has already been processed: {MessageId}", DateTime.UtcNow, messageId);
            return;
        }

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var error = await SendEmailNotification(message, messageId, cancellationToken);
        // If there is an error, throw an exception to fail the message consuming
        if(error != null)
            throw new Exception(error);

        await _context.MarkMessageAsConsumed(messageId, nameof(EmailNotificationConsumer));
        await transaction.CommitAsync(CancellationToken.None);
        var messageJson = JsonSerializer.Serialize(message);
        _logger.LogInformation("{Date} Message consumed: {Message}", DateTime.UtcNow, messageJson);
    }

    private async Task<string?> SendEmailNotification(EmailNotificationMessage message, string messageId, CancellationToken cancellationToken)
    {
        // The messageId is the email Id
        var emailId = new EmailId(Guid.Parse(messageId));
        var email = await _context.Emails.SingleAsync(e => e.Id.Equals(emailId), cancellationToken);
        // TODO: create a wrapper for date, to make tests possible
        email.DateSent = DateTime.UtcNow;

        return await _emailService.SendEmailAsync(message.Subject, message.Body, message.Recipients, cancellationToken);
    }
}

public interface IEmailNotificationConsumer
{
    Task Consume(EmailNotificationMessage message, CapHeader capHeader, CancellationToken cancellationToken);
}