using Mediator;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Domain.Common;
using IPublisher = SiteWatcher.Domain.Common.Services.IPublisher;

namespace SiteWatcher.Infra.Extensions;

public static class ContextExtensions
{
    public static async Task DispatchDomainEventsAndMessages(this IMediator mediator, DbContext context,
        IPublisher publisher, CancellationToken ct)
    {
        var models = context
            .ChangeTracker
            .Entries<IBaseModel>()
            .Where(m => m.Entity.DomainEvents.Count != 0 || m.Entity.Messages.Count != 0)
            .Select(m => m.Entity)
            .ToArray();

        if(models.Length == 0)
            return;

        foreach (var model in models)
        {
            foreach (var domainEvent in model.DomainEvents)
                await mediator.Publish(domainEvent, ct);

            foreach (var message in model.Messages)
                await publisher.PublishAsync(message.GetType().Name, message, ct);

            model.ClearDomainEvents();
            model.ClearMessages();
        }
    }
}