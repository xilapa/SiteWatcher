using MediatR;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Emails.Events;
using IPublisher = SiteWatcher.Domain.Common.Services.IPublisher;

namespace SiteWatcher.Application.Emails.EventHandlers;

public class EmailCreatedEventHandler : INotificationHandler<EmailCreatedEvent>
{
    private readonly IPublisher _publishService;

    public EmailCreatedEventHandler(IPublisher publishService)
    {
        _publishService = publishService;
    }
    public async Task Handle(EmailCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _publishService.PublishAsync(RoutingKeys.MailMessage, notification.MailMessage, cancellationToken);
    }
}