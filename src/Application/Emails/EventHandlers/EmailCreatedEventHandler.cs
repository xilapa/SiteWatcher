using MediatR;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Emails.Events;

namespace SiteWatcher.Application.Emails.EventHandlers;

public class EmailCreatedEventHandler : INotificationHandler<EmailCreatedEvent>
{
    private readonly IPublishService _publishService;

    public EmailCreatedEventHandler(IPublishService publishService)
    {
        _publishService = publishService;
    }
    public async Task Handle(EmailCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _publishService.WithPublisher(
            async p => await p.PublishAsync(RoutingKeys.MailMessage, notification.MailMessage, cancellationToken),
            cancellationToken);
    }
}