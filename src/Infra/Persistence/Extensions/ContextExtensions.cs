using Mediator;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Infra.Extensions;

public static class ContextExtensions
{
    public static async Task DispatchDomainEventsAndMessages(this IMediator mediator, DbContext context, CancellationToken ct)
    {
        var models = context
            .ChangeTracker
            .Entries<IBaseModel>()
            .Where(m => m.Entity.DomainEvents.Count != 0)
            .Select(m => m.Entity)
            .ToArray();

        if(models.Length == 0)
            return;

        foreach (var model in models)
        {
            foreach (var domainEvent in model.DomainEvents)
                await mediator.Publish(domainEvent, ct);

            model.ClearDomainEvents();
        }
    }
}